using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Srs.Api.Domain;

namespace Srs.Api.Controllers;

public class AuthorizeAdminAttribute : AuthorizeAttribute
{
	public AuthorizeAdminAttribute()
	{
		Roles = Constants.ADMIN_ROLE_NAME;
	}
}

[ApiController]
[Route("[controller]/[action]")]
[AuthorizeAdmin]
public class DbAdminController : ControllerBase
{
	private readonly SrsDbContext _db;

	public DbAdminController(SrsDbContext db)
	{
		_db = db;
	}

	[HttpGet]
	public async Task RecreateDatabase()
	{
		await _db.Database.EnsureDeletedAsync();
		await _db.Database.EnsureCreatedAsync();
	}

	[HttpGet]
	public async Task SeedProducts(int count = 100)
	{
		var products = new Faker<Product>()
			.RuleFor(x => x.Name, x => x.Commerce.ProductName())
			.RuleFor(x => x.Description, x => x.Commerce.ProductDescription())
			.RuleFor(x => x.UnitPrice, x => x.Random.Decimal() * 100_000)
			.Generate(count);
		await _db.Products.AddRangeAsync(products);

		await _db.SaveChangesAsync();
	}


	[HttpGet]
	public async Task SeedUsers()
	{
		var adminRole = new UserRole { Id = 0, Name = Constants.ADMIN_ROLE_NAME};
		var agentRole = new UserRole { Id = 0, Name = Constants.AGENT_ROLE_NAME };
		await _db.UserRoles.AddRangeAsync(adminRole, agentRole);

		// todo: hash password
		var adminUser = new User { Id = 0, Name = "admin", PasswordHash = "admin", Roles = new List<UserRole> { adminRole } };
		var agentUsers = new Faker<User>()
			.Rules((f, u) =>
			{
				u.Name = $"agent{f.IndexVariable++}";
				u.PasswordHash = u.Name;
				u.ReportingManager = adminUser;
				u.Roles = new List<UserRole> { agentRole };

			})
			.Generate(10);
		await _db.Users.AddRangeAsync(agentUsers.Prepend(adminUser));
		
		await _db.SaveChangesAsync();
	}
}

[ApiController]
[Route("[controller]/[action]")]
public class AuthController : ControllerBase
{
	private readonly SrsDbContext _db;

	public record TokenRequest(string UserName, string Password);
	public record TokenResponse(string Token, DateTime ExpiresAt);

	public AuthController(SrsDbContext db)
	{
		_db = db;
	}

	[HttpPost]
	public async Task<TokenResponse> Token([FromBody] TokenRequest request)
	{
		// todo: hash password
		var passwordHash = request.Password;
		var user = await _db.Users.SingleOrDefaultAsync(x => x.Name.Equals(request.UserName) && x.PasswordHash.Equals(passwordHash));

		if (user == null) throw new Exception("Invalid user credentials");

		await _db.Entry(user)
			.Collection(x => x.Roles)
			.LoadAsync();

		var handler = new JwtSecurityTokenHandler();
		var secret = Constants.JWT_DEFAULT_SECRET;
		var now = DateTime.UtcNow;
		var expires = now.AddMinutes(5);
		var claims = new List<Claim>
		{
			new("id", user.Id.ToString()),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		}.Concat(user.Roles.Select(x => new Claim(ClaimTypes.Role, x.Name)));

		var descriptor = new SecurityTokenDescriptor
		{
			Issuer = Constants.JWT_VALID_ISSUER,
			Audience = Constants.JWT_VALID_AUDIENCE,
			Subject = new ClaimsIdentity(claims),
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature),
		};

		var token = handler.CreateToken(descriptor);
		var jwt = handler.WriteToken(token);

		return new TokenResponse(jwt, expires);
	}
}

[ApiController]
[Route("[controller]")]
public class ProductsController
{
	private readonly SrsDbContext _db;

	public ProductsController(SrsDbContext db)
	{
		_db = db;
	}

	// todo: use dto
	[Authorize]
	[HttpGet]
	public async Task<List<Product>> GetAll()
	{
		var products = await _db.Products.ToListAsync();
		return products;
	}
}