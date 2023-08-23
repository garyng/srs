using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Srs.Api.Controllers.Auth;

namespace Srs.Tests;

[ApiController]
[Route("[controller]/[action]")]
public class AuthTestController
{
	[AuthorizeAdmin]
	[HttpGet]
	public string AdminOnly()
	{
		return "admin only";
	}

	[Authorize]
	[HttpGet]
	public string Authenticated()
	{
		return "authenticated";
	}
}

public class AuthTests : IntegrationTestsBase
{
	protected override void ConfigureBuilder(IWebHostBuilder builder)
	{
		builder.ConfigureServices(services => services.AddControllers()
			.AddApplicationPart(typeof(AuthTestController).Assembly)
		);
	}

	[Test]
	public async Task Can_Authenticate()
	{
		// Arrange

		// Act
		var result = await _mediator.Send(new AuthenticateAsTestAdmin());

		// Assert
		result.Token.Should().NotBeNull();
	}

	[TestCase("/AuthTest/AdminOnly")]
	[TestCase("AuthTest/Authenticated")]
	public async Task Admin_CanAccessAllEndpoints(string url)
	{
		// Arrange
		await _mediator.Send(new AuthenticateAsTestAdmin());

		// Act
		var result = () => _httpClient.GetStringAsync(url);

		// Assert
		(await result.Should().NotThrowAsync())
			.Which.Should().NotBeNullOrEmpty();

	}

	[TestCase("/AuthTest/AdminOnly", true)]
	[TestCase("AuthTest/Authenticated", false)]
	public async Task Agent_CanOnlyAccessNonAdminEndpoints(string url, bool shouldThrow)
	{
		// Arrange
		await _mediator.Send(new AuthenticateAsTestAgent());

		// Act
		var result = () => _httpClient.GetStringAsync(url);

		// Assert
		if (shouldThrow)
		{
			await result.Should().ThrowAsync<Exception>();
		} else
		{
			(await result.Should().NotThrowAsync())
				.Which.Should().NotBeNullOrEmpty();
		}

	}
}