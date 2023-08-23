using FluentAssertions;
using Srs.ApiClient;

namespace Srs.Tests;

public class ReportControllerTests : IntegrationTestsBase
{
	private ReportClient _client;

	[SetUp]
	public async Task SetUp()
	{
		await _mediator.Send(new AuthenticateAsTestAdmin());
		await new DbAdminClient("", _httpClient).SeedDatabaseAsync(false);
		_client = new ReportClient("", _httpClient);
	}

	[Test]
	public async Task Can_GenerateYearlySalesReport()
	{
		// Arrange

		// Act
		var result = await _client.YearlySalesAsync(2020);

		// Assert
		result.Should().NotBeEmpty();
	}
}