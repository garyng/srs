using MediatR;
using Microsoft.EntityFrameworkCore;
using Srs.Api.Domain;

namespace Srs.Api.Controllers.Report;

public record GetYearlySalesReport(int Year) : IRequest<List<YearlySalesReportResponseDto>>;

public class GetYearlySalesReportRequestHandler : IRequestHandler<GetYearlySalesReport, List<YearlySalesReportResponseDto>>
{
	private readonly SrsDbContext _db;

	public GetYearlySalesReportRequestHandler(SrsDbContext db)
	{
		_db = db;
	}

	public async Task<List<YearlySalesReportResponseDto>> Handle(GetYearlySalesReport request, CancellationToken cancellationToken)
	{
		var results = (await _db.SaleTransactions
				.Where(x => x.CreatedAt.Year == request.Year)
				.OrderBy(x => x.CreatedAt)
				.GroupBy(x => new { year = x.CreatedAt.Year, month = x.CreatedAt.Month })
				.Select(x => new
				{
					Date = x.Key.year + "-" + x.Key.month.ToString().PadLeft(2, '0'),
					TotalSales = x.Count(),
					ItemCounts = x.SelectMany(y => y.Items.Select(z => z.Quantity)),
					TotalProducts = x.SelectMany(y => y.Items.Select(z => z.ProductId)).Distinct().Count(),
					TotalAmount = x.Sum(y => y.Total)
				})
				.ToListAsync(cancellationToken))
			.Select(x => new YearlySalesReportResponseDto
			{
				Date = x.Date,
				TotalSales = x.TotalSales,
				TotalItems = x.ItemCounts.Sum(),
				TotalProducts = x.TotalProducts,
				TotalAmount = x.TotalAmount
			})
			.ToList();

		return results;
	}
}