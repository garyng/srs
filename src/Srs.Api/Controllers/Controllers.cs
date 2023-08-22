using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Srs.Api.Controllers.Auth;
using Srs.Api.Domain;

namespace Srs.Api.Controllers;

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