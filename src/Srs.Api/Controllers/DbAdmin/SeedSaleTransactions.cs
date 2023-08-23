using Bogus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Srs.Api.Domain;

namespace Srs.Api.Controllers.DbAdmin;

public record SeedSaleTransactions(int Count = 100) : IRequest<List<SaleTransaction>>;

public class SeedSaleTransactionsRequestHandler : IRequestHandler<SeedSaleTransactions, List<SaleTransaction>>
{
	private readonly SrsDbContext _db;
	private readonly IMediator _mediator;

	public SeedSaleTransactionsRequestHandler(SrsDbContext db, IMediator mediator)
	{
		_db = db;
		_mediator = mediator;
	}

	public async Task<List<SaleTransaction>> Handle(SeedSaleTransactions request, CancellationToken cancellationToken)
	{
		if (!await _db.Products.AnyAsync(cancellationToken))
		{
			await _mediator.Send(new SeedProducts(request.Count), cancellationToken);
		}
		var products = await _db.Products.ToListAsync(cancellationToken);

		var saleItems = new Faker<SaleItem>()
			.Rules((f, x) =>
			{
				x.Product = f.PickRandom(products);
				x.Quantity = f.Random.Int(0, 10000);
				x.Total = x.Product.UnitPrice * x.Quantity;
			});
		var users = await _db.Users.ToListAsync(cancellationToken);
		var saleTransaction = new Faker<SaleTransaction>()
			.Rules((f, x) =>
			{
				x.Items = saleItems.Generate(f.Random.Int(0, 100));
				x.Total = x.Items.Sum(x => x.Total);
				x.User = f.PickRandom(users);
				x.CreatedAt = f.Date.Between(new DateTime(2020, 1, 1), new DateTime(2025, 1, 1));
				x.LastUpdatedAt = f.Date.Between(x.CreatedAt, DateTime.Now);
			});

		var sales = saleTransaction.Generate(request.Count);
		await _db.SaleTransactions.AddRangeAsync(sales, cancellationToken);
		await _db.SaveChangesAsync(cancellationToken);

		return sales;
	}
}
