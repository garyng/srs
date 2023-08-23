using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Srs.ApiClient;

namespace Srs.Tests;

public class SaleTransactionTests : IntegrationTestsBase
{
	private SaleTransactionClient _client;
	private List<Product> _products;

	[SetUp]
	public async Task SetUp()
	{
		await _mediator.Send(new AuthenticateAsTestAdmin());
		await new DbAdminClient("", _httpClient).SeedDatabaseAsync(false);
		_products = (await new ProductsClient("", _httpClient).GetAllAsync()).ToList();
		_client = new SaleTransactionClient("", _httpClient);
	}

	[Test]
	public async Task Can_Insert()
	{
		// Arrange
		var salesCount = await _db.SaleTransactions.CountAsync();
		var itemsCount = await _db.SaleItems.CountAsync();

		var request = new SaleTransactionRequestDto
		{
			Items = new List<SaleItemRequestDto>
			{
				new() { ProductId = _products[0].Id, Quantity = 10 },
				new() { ProductId = _products[1].Id, Quantity = 20 },
				new() { ProductId = _products[2].Id, Quantity = 30 }
			}
		};

		// Act
		var result = await _client.PostAsync(request);

		// Assert
		result.Id.Should().NotBe(0);
		(await _db.SaleTransactions.CountAsync()).Should().Be(salesCount + 1);
		(await _db.SaleItems.CountAsync()).Should().Be(itemsCount + 3);
	}

	[Test]
	public async Task Can_GetAll()
	{
		// Arrange
		var expected = await _db.SaleTransactions.CountAsync();

		// Act
		var result = await _client.GetAllAsync();

		// Assert
		result.Should().HaveCount(expected);
	}
}