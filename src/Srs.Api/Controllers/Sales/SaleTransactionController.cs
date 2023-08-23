using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Srs.Api.Domain;

namespace Srs.Api.Controllers.Sales;

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