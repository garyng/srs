using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Srs.ApiClient;

namespace Srs.Tests;

public class SeedingTests : IntegrationTestsBase
{
	private DbAdminClient _dbAdminClient;

	[SetUp]
	public async Task SetUp()
	{
		await _mediator.Send(new AuthenticateAsTestAdmin());
		_dbAdminClient = new DbAdminClient("", _httpClient);
	}

	[Test]
	public async Task Can_SeedProducts()
	{
		// Arrange
		var productsClient = new ProductsClient("", _httpClient);
		var currentCount = (await productsClient.GetAllAsync()).Count;
		var seedCount = 100;
		var expectedCount = currentCount + seedCount;

		// Act
		await _dbAdminClient.SeedProductsAsync(new ApiClient.SeedProducts
		{
			Count = seedCount
		});

		// Assert
		var resultCount = (await productsClient.GetAllAsync()).Count;
		resultCount.Should().Be(expectedCount);
	}

	[Test]
	public async Task Can_SeedUsers()
	{
		// Arrange
		var current = await _db.Users.CountAsync();
		var agentUserCount = 55;
		var expectedCount = current + agentUserCount + 1;

		// Act
		await _dbAdminClient.SeedUsersAsync(new ApiClient.SeedUsers
		{
			AgentUserCount = agentUserCount
		});
		var result = await _db.Users.CountAsync();

		// Assert
		result.Should().Be(expectedCount);
	}

	[Test]
	public async Task Can_SeedSaleTransactions()
	{
		// Arrange
		var current = await _db.SaleTransactions.CountAsync();
		var transactionsCount = 100;
		var expectedCount = current + transactionsCount;

		// Act
		await _dbAdminClient.SeedSaleTransactionsAsync(new ApiClient.SeedSaleTransactions
		{
			Count = expectedCount
		});
		var result = await _db.SaleTransactions.CountAsync();

		// Assert
		result.Should().Be(expectedCount);
	}
}