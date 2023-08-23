using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Srs.Api.Domain;

namespace Srs.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ProductsController
{
	private readonly SrsDbContext _db;

	public ProductsController(SrsDbContext db)
	{
		_db = db;
	}

	[Authorize]
	[HttpGet]
	public async Task<List<Product>> GetAll()
	{
		var products = await _db.Products.ToListAsync();
		return products;
	}
}

[ApiController]
[Route("[controller]/[action]")]
public class SaleTransactionController
{
	private readonly SrsDbContext _db;

	public SaleTransactionController(SrsDbContext db)
	{
		_db = db;
	}

	[Authorize]
	[HttpGet]
	public async Task<List<SaleTransaction>> GetAll()
	{
		var transactions = await _db.SaleTransactions.ToListAsync();
		return transactions;
	}
}