using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Srs.Api.Domain;

namespace Srs.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
	private static readonly string[] Summaries = new[]
	{
		"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
	};

	private readonly ILogger<WeatherForecastController> _logger;

	public WeatherForecastController(ILogger<WeatherForecastController> logger)
	{
		_logger = logger;
	}

	[HttpGet(Name = "GetWeatherForecast")]
	public IEnumerable<WeatherForecast> Get()
	{
		return Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateTime.Now.AddDays(index),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
	}
}

[ApiController]
[Route("[controller]/[action]")]
public class DbAdminController : ControllerBase
{
	private readonly SrsDbContext _db;

	public DbAdminController(SrsDbContext db)
	{
		_db = db;
	}

	[HttpGet]
	public async Task RecreateDatabase()
	{
		await _db.Database.EnsureDeletedAsync();
		await _db.Database.EnsureCreatedAsync();
	}

	[HttpGet]
	public async Task SeedProducts(int count = 100)
	{
		var products = new Faker<Product>()
			.RuleFor(x => x.Name, x => x.Commerce.ProductName())
			.RuleFor(x => x.Description, x => x.Commerce.ProductDescription())
			.RuleFor(x => x.UnitPrice, x => x.Random.Decimal() * 100_000)
			.Generate(count);
		await _db.Products.AddRangeAsync(products);
		await _db.SaveChangesAsync();
	}
}

[ApiController]
[Route("[controller]")]
public class ProductsController
{
	private readonly SrsDbContext _db;

	public ProductsController(SrsDbContext db)
	{
		_db = db;
	}

	// todo: use dto
	[HttpGet]
	public async Task<List<Product>> GetAll()
	{
		var products = await _db.Products.ToListAsync();
		return products;
	}
}