using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Srs.Api.Controllers.Report;

[ApiController]
[Route("[controller]/[action]")]
[Authorize]
public class ReportController
{
	private readonly IMediator _mediator;

	public ReportController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[HttpPost]
	public async Task<List<YearlySalesReportResponseDto>> YearlySales(int year)
	{
		return await _mediator.Send(new GetYearlySalesReport(year));
	}
}