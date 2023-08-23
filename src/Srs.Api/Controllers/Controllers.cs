using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;
using Srs.Api.Domain;

namespace Srs.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController
{
	private readonly SrsDbContext _db;

	public ProductsController(SrsDbContext db)
	{
		_db = db;
	}

	[Authorize]
	[HttpGet("[action]")]
	public async Task<List<Product>> GetAll()
	{
		var products = await _db.Products.ToListAsync();
		return products;
	}
}

public class SaleTransactionRequestDto
{
	public List<SaleItemRequestDto> Items { get; set; }
}

public class SaleItemRequestDto
{
	public int ProductId { get; set; }
	public int Quantity { get; set; }
}

public class SaleTransactionResponseDto
{
	public int Id { get; set; }
	public decimal Total { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime LastUpdatedAt { get; set; }

	public List<SaleItemResponseDto> Items { get; set; }
	public UserResponseDto User { get; set; }
}

public class SaleItemResponseDto
{
	public int Id { get; set; }
	public ProductResponseDto Product { get; set; }
	public int Quantity { get; set; }
	public decimal Total { get; set; }
}

public class ProductResponseDto
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string? Description { get; set; }
	public decimal UnitPrice { get; set; }
}

public class UserResponseDto
{
	public int Id { get; set; }
	public string Name { get; set; }
}

public record InsertSaleTransaction(SaleTransactionRequestDto Request) : IRequest<SaleTransactionResponseDto>;

public class InsertSaleTransactionRequestHandler : IRequestHandler<InsertSaleTransaction, SaleTransactionResponseDto>
{
	private readonly SrsDbContext _db;
	private readonly IMediator _mediator;

	public InsertSaleTransactionRequestHandler(SrsDbContext db, IMediator mediator)
	{
		_db = db;
		_mediator = mediator;
	}

	public async Task<SaleTransactionResponseDto> Handle(InsertSaleTransaction request, CancellationToken cancellationToken)
	{
		var user = await _mediator.Send(new GetCurrentUser(), cancellationToken);
		var dto = request.Request;
		var productIds = dto.Items.Select(x => x.ProductId).ToList();

		var products = await _db
			.Products
			.Where(x => productIds.Contains(x.Id))
			.ToDictionaryAsync(x => x.Id, x => x, cancellationToken);

		var items = dto.Items.Select(x => new SaleItem
		{
			ProductId = x.ProductId,
			Quantity = x.Quantity,
			Total = x.Quantity * products[x.ProductId].UnitPrice
		}).ToList();

		var sale = new SaleTransaction
		{
			CreatedAt = DateTime.UtcNow,
			LastUpdatedAt = DateTime.UtcNow,
			Items = items,
			Total = items.Sum(x => x.Total),
			User = user
		};

		_db.SaleTransactions.Add(sale);
		await _db.SaveChangesAsync(cancellationToken);

		return sale.ToResponseDto();
	}
}

[Mapper]
public static partial class SaleTransactionMapper
{
	public static partial SaleTransactionResponseDto ToResponseDto(this SaleTransaction transaction);
}

public record GetCurrentUser : IRequest<User>;

public class GetCurrentUserRequestHandler : IRequestHandler<GetCurrentUser, User>
{
	private readonly IHttpContextAccessor _accessor;
	private readonly SrsDbContext _db;

	public GetCurrentUserRequestHandler(IHttpContextAccessor accessor, SrsDbContext db)
	{
		_accessor = accessor;
		_db = db;
	}

	public async Task<User> Handle(GetCurrentUser request, CancellationToken cancellationToken)
	{
		var sub = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (sub == null) throw new UnauthorizedAccessException("Invalid user");
		if (!int.TryParse(sub, out var id)) throw new UnauthorizedAccessException("Invalid user");

		var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
		if (user == null) throw new UnauthorizedAccessException("Invalid user");
		return user;
	}
}

public record GetAllSaleTransactions : IRequest<List<SaleTransactionResponseDto>>;

public class GetAllSaleTransactionsRequestHandler : IRequestHandler<GetAllSaleTransactions, List<SaleTransactionResponseDto>>
{
	private readonly SrsDbContext _db;

	public GetAllSaleTransactionsRequestHandler(SrsDbContext db)
	{
		_db = db;
	}

	public async Task<List<SaleTransactionResponseDto>> Handle(GetAllSaleTransactions request, CancellationToken cancellationToken)
	{
		var sales = await _db.SaleTransactions
			.Include(x => x.User)
			.Include(x => x.Items)
			.ThenInclude(x => x.Product)
			.ToListAsync(cancellationToken);

		return sales.Select(x => x.ToResponseDto())
			.ToList();
	}
}

[ApiController]
[Route("[controller]")]
[Authorize]
public class SaleTransactionController
{
	private readonly SrsDbContext _db;
	private readonly IMediator _mediator;

	public SaleTransactionController(SrsDbContext db, IMediator mediator)
	{
		_db = db;
		_mediator = mediator;
	}

	[HttpGet]
	public async Task<List<SaleTransactionResponseDto>> GetAll()
	{
		return await _mediator.Send(new GetAllSaleTransactions());
	}

	[HttpPost]
	public async Task<SaleTransactionResponseDto> Post(SaleTransactionRequestDto request)
	{
		return await _mediator.Send(new InsertSaleTransaction(request));
	}
}