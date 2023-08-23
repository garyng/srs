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
using Microsoft.AspNetCore.Hosting;
using GenerateToken = Srs.ApiClient.GenerateToken;
using GenerateTokenResult = Srs.ApiClient.GenerateTokenResult;

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
				{
					ConfigureBuilder(builder);
					builder.ConfigureServices(services =>
					{
						services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<ITestMediatorMarker>());
						services.AddSingleton<TestHttpClientAccessor>();
					});
				});
			_scope = _webAppFactory.Services.CreateScope();
			RebuildHttpClient();
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

		protected void RebuildHttpClient()
		{
			_httpClient = _webAppFactory.CreateClient();
			_httpClientAccessor = _scope.ServiceProvider.GetRequiredService<TestHttpClientAccessor>();
			_httpClientAccessor.Current = _httpClient;
		}

		protected virtual void ConfigureBuilder(IWebHostBuilder builder)
		{
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
		private readonly IMediator _mediator;

		public AuthenticateAsTestAdminRequestHandler(IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task<GenerateTokenResult> Handle(AuthenticateAsTestAdmin request, CancellationToken cancellationToken)
		{
			return await _mediator.Send(new AuthenticateAsTestUser(Constants.ADMIN_ROLE_NAME, "test-admin", "test-admin"), cancellationToken);
		}
	}

	public record AuthenticateAsTestAgent : IRequest<GenerateTokenResult>;
	public class AuthenticateAsTestAgentRequestHandler : IRequestHandler<AuthenticateAsTestAgent, GenerateTokenResult>
	{
		private readonly IMediator _mediator;

		public AuthenticateAsTestAgentRequestHandler(IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task<GenerateTokenResult> Handle(AuthenticateAsTestAgent request, CancellationToken cancellationToken)
		{
			return await _mediator.Send(new AuthenticateAsTestUser(Constants.AGENT_ROLE_NAME, "test-agent", "test-agent"), cancellationToken);

		}
	}

	public record AuthenticateAsTestUser(string Role, string UserName, string Password) : IRequest<GenerateTokenResult>;

	public class AuthenticateAsTestUserRequestHandler : IRequestHandler<AuthenticateAsTestUser, GenerateTokenResult>
	{
		private readonly SrsDbContext _db;
		private readonly IMediator _mediator;
		private readonly HttpClient _httpClient;

		public AuthenticateAsTestUserRequestHandler(SrsDbContext db, IMediator mediator, TestHttpClientAccessor httpClient)
		{
			_db = db;
			_mediator = mediator;
			_httpClient = httpClient.Current;
		}

		public async Task<GenerateTokenResult> Handle(AuthenticateAsTestUser request, CancellationToken cancellationToken)
		{
			var role = await _db.UserRoles.FirstOrDefaultAsync(x => x.Name == request.Role, cancellationToken: cancellationToken);
			role ??= new UserRole
			{
				Id = 0,
				Name = request.Role
			};
			var user = new User
			{
				Id = 0,
				Name = request.UserName,
				PasswordHash = await _mediator.Send(new GetHashOf(request.Password), cancellationToken),
				Roles = new List<UserRole> { role }
			};
			_db.Users.Add(user);
			await _db.SaveChangesAsync(cancellationToken);

			var client = new AuthClient("", _httpClient);
			var result = await client.TokenAsync(new GenerateToken
			{
				UserName = request.UserName,
				Password = request.Password
			}, cancellationToken);

			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
			return result;

		}
	}

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
}