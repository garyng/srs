using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Srs.Api;
using Srs.Api.Domain;
using Srs.ApiClient;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Srs.Api.Controllers.Auth;
using GenerateToken = Srs.ApiClient.GenerateToken;
using GenerateTokenResult = Srs.ApiClient.GenerateTokenResult;
using Product = Srs.ApiClient.Product;

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
			await _db.Database.EnsureCreatedAsync();
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
			role ??= new Api.Domain.UserRole
			{
				Id = 0,
				Name = request.Role
			};
			var user = new Api.Domain.User
			{
				Id = 0,
				Name = request.UserName,
				PasswordHash = await _mediator.Send(new GetHashOf(request.Password), cancellationToken),
				Roles = new List<Api.Domain.UserRole> { role }
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
	}
}