using Bogus;
using MediatR;
using Srs.Api.Domain;

namespace Srs.Api.Controllers.DbAdmin;

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
