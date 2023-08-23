using System.Security.Cryptography;
using System.Text;
using Bogus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Srs.Api.Controllers.Auth;
using Srs.Api.Domain;

namespace Srs.Api.Controllers;

public record RecreateDatabase : IRequest;

public class RecreateDatabaseRequestHandler : IRequestHandler<RecreateDatabase>
{
	private readonly SrsDbContext _db;

	public RecreateDatabaseRequestHandler(SrsDbContext db)
	{
		_db = db;
	}

	public async Task Handle(RecreateDatabase request, CancellationToken cancellationToken)
	{
		await _db.Database.EnsureDeletedAsync(cancellationToken);
		await _db.Database.EnsureCreatedAsync(cancellationToken);
	}
}

public record SeedUsers(
	string AdminUserName = "admin",
	string AdminPassword = "admin",
	string AgentUserNamePrefix = "agent",
	string AgentUserPasswordPrefix = "agent",
	int AgentUserCount = 10
) : IRequest<SeedUsersResult>;

public record SeedUsersResult(
	User Admin,
	List<User> Agents
);

public class SeedUsersRequestHandler : IRequestHandler<SeedUsers, SeedUsersResult>
{
	private readonly SrsDbContext _db;
	private readonly IMediator _mediator;

	public SeedUsersRequestHandler(SrsDbContext db, IMediator mediator)
	{
		_db = db;
		_mediator = mediator;
	}

	public async Task<SeedUsersResult> Handle(SeedUsers request, CancellationToken cancellationToken)
	{
		var adminRole = new UserRole { Id = 0, Name = Constants.ADMIN_ROLE_NAME };
		var agentRole = new UserRole { Id = 0, Name = Constants.AGENT_ROLE_NAME };
		await _db.UserRoles.AddRangeAsync(adminRole, agentRole);

		var adminUser = new User
		{
			Id = 0,
			Name = request.AdminUserName,
			PasswordHash = await _mediator.Send(new GetHashOf(request.AdminPassword), cancellationToken),
			Roles = new List<UserRole> { adminRole }
		};
		var agentUsers = new Faker<User>()
			.Rules((f, u) =>
			{
				u.Name = $"{request.AgentUserNamePrefix}{f.IndexVariable}";
				u.PasswordHash = _mediator.Send(new GetHashOf($"{request.AgentUserPasswordPrefix}{f.IndexVariable}"), cancellationToken).GetAwaiter().GetResult();
				u.ReportingManager = adminUser;
				u.Roles = new List<UserRole> { agentRole };
				f.IndexVariable++;
			})
			.Generate(request.AgentUserCount);
		await _db.Users.AddRangeAsync(agentUsers.Prepend(adminUser), cancellationToken);

		await _db.SaveChangesAsync(cancellationToken);

		return new SeedUsersResult(adminUser, agentUsers);
	}
}

public record GetHashOf(string PlainText) : IRequest<string>;

public class GetHashOfRequestHandler : IRequestHandler<GetHashOf, string>
{
	public async Task<string> Handle(GetHashOf request, CancellationToken cancellationToken)
	{
		var text = Encoding.UTF8.GetBytes(request.PlainText);
		var hash = SHA256.HashData(text);
		return Convert.ToHexString(hash);
	}
}

public record SeedProducts(int Count = 100) : IRequest<List<Product>>;

public class SeedProductsRequestHandler : IRequestHandler<SeedProducts, List<Product>>
{
	private readonly SrsDbContext _db;

	public SeedProductsRequestHandler(SrsDbContext db)
	{
		_db = db;
	}

	public async Task<List<Product>> Handle(SeedProducts request, CancellationToken cancellationToken)
	{
		var products = new Faker<Product>()
			.RuleFor(x => x.Name, x => x.Commerce.ProductName())
			.RuleFor(x => x.Description, x => x.Commerce.ProductDescription())
			.RuleFor(x => x.UnitPrice, x => x.Random.Decimal() * 100_000)
			.Generate(request.Count);

		await _db.Products.AddRangeAsync(products, cancellationToken);

		await _db.SaveChangesAsync(cancellationToken);

		return products;
	}
}


[ApiController]
[Route("[controller]/[action]")]
[AuthorizeAdmin]
public class DbAdminController : ControllerBase
{
	private readonly IMediator _mediator;

	public DbAdminController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[HttpPost]
	public async Task RecreateDatabase()
	{
		await _mediator.Send(new RecreateDatabase());
	}

	[HttpPost]
	public async Task SeedProducts(SeedProducts request)
	{
		await _mediator.Send(request);
	}

	[HttpPost]
	public async Task SeedUsers(SeedUsers request)
	{
		await _mediator.Send(request);
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