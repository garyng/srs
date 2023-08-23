using Bogus;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Srs.Api;
using Srs.Api.Controllers;
using Srs.Api.Domain;
using Srs.ApiClient;
using System.Net.Http.Headers;

namespace Srs.Tests
{
	public class IntegrationTestsBase
	{
		protected WebApplicationFactory<Program> _webAppFactory;
		protected IServiceScope _scope;
		protected SrsDbContext _db;
		protected IDbContextTransaction _transaction;
		protected HttpClient _httpClient;
		protected IMediator _mediator;
		protected TestHttpClientAccessor _httpClientAccessor;

		[SetUp]
		public async Task IntegrationTestsBaseSetUp()
		{
			RuntimeContext.IsIntegrationTests = true;
			_webAppFactory = new WebApplicationFactory<Program>()
				.WithWebHostBuilder(builder =>
					builder.ConfigureServices(services =>
					{
						services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<ITestMediatorMarker>());
						services.AddSingleton<TestHttpClientAccessor>();
					}));
			_scope = _webAppFactory.Services.CreateScope();
			_httpClient = _webAppFactory.CreateClient();
			_httpClientAccessor = _scope.ServiceProvider.GetRequiredService<TestHttpClientAccessor>();
			_httpClientAccessor.Current = _httpClient;
			_db = _scope.ServiceProvider.GetRequiredService<SrsDbContext>();
			_transaction = await _db.Database.BeginTransactionAsync();
			_mediator = _scope.ServiceProvider.GetRequiredService<IMediator>();
		}

		[TearDown]
		public async Task IntegrationTestsBaseTearDown()
		{
			await _transaction.RollbackAsync();
			_scope.Dispose();
		}

		protected async Task AuthenticateAsAdmin()
		{
			var result = await _mediator.Send(new AuthenticateAsTestAdmin());
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
		}
	}

	public interface ITestMediatorMarker
	{

	}

	public class TestHttpClientAccessor
	{
		public HttpClient Current { get; internal set; }
	}

	public record AuthenticateAsTestAdmin : IRequest<GenerateTokenResult>;
	public class AuthenticateAsTestAdminRequestHandler : IRequestHandler<AuthenticateAsTestAdmin, GenerateTokenResult>
	{
		private readonly SrsDbContext _db;
		private readonly IMediator _mediator;
		private readonly HttpClient _httpClient;

		public AuthenticateAsTestAdminRequestHandler(SrsDbContext db, IMediator mediator, TestHttpClientAccessor httpClient)
		{
			_db = db;
			_mediator = mediator;
			_httpClient = httpClient.Current;
		}

		public async Task<GenerateTokenResult> Handle(AuthenticateAsTestAdmin request, CancellationToken cancellationToken)
		{
			var adminRole = await _db.UserRoles.FirstOrDefaultAsync(x => x.Name == Constants.ADMIN_ROLE_NAME, cancellationToken: cancellationToken);
			adminRole ??= new UserRole
			{
				Id = 0,
				Name = Constants.ADMIN_ROLE_NAME
			};
			var userName = "test-admin";
			var password = "test-admin";
			var user = new User
			{
				Id = 0,
				Name = userName,
				PasswordHash = await _mediator.Send(new GetHashOf(password)),
				Roles = new List<UserRole> { adminRole }
			};
			_db.Users.Add(user);
			await _db.SaveChangesAsync(cancellationToken);

			var client = new AuthClient("", _httpClient);
			var result = await client.TokenAsync(new GenerateToken
			{
				UserName = userName,
				Password = password
			}, cancellationToken);
			return result;
		}
	}

	public class AuthTests : IntegrationTestsBase
	{
		[Test]
		public async Task Can_Authenticate()
		{
			// Arrange

			// Act
			var result = await _mediator.Send(new AuthenticateAsTestAdmin());

			// Assert
			result.Token.Should().NotBeNull();
		}
	}

	public class SeedingTests : IntegrationTestsBase
	{
		private DbAdminClient _dbAdminClient;

		[SetUp]
		public async Task SetUp()
		{
			await AuthenticateAsAdmin();
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

	}
}