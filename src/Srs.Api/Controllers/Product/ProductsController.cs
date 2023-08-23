using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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