using MediatR;
using Microsoft.EntityFrameworkCore;
using Srs.Api.Domain;

namespace Srs.Api.Controllers.Sales;

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
