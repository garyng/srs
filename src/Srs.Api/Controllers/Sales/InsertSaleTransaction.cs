using MediatR;
using Microsoft.EntityFrameworkCore;
using Srs.Api.Controllers.Auth;
using Srs.Api.Domain;

namespace Srs.Api.Controllers.Sales;

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
