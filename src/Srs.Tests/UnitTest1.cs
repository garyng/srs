using Microsoft.AspNetCore.Mvc.Testing;
using Srs.ApiClient;

namespace Srs.Tests
{
	public class Tests
	{
		[SetUp]
		public async Task Setup()
		{
			var webAppFactory = new WebApplicationFactory<Program>();
			var httpClient = webAppFactory.CreateClient();
			var authApiClient = new AuthClient("", httpClient);

			var tokenResult = await authApiClient.TokenAsync(new TokenRequest
			{
				UserName = "admin",
				Password = "admin"
			});

			Console.WriteLine(tokenResult.Token);

			//var response = await httpClient.PostAsJsonAsync("/Auth/Token", new AuthController.TokenRequest("admin", "admin"));
			//var token = await response.Content.ReadFromJsonAsync<AuthController.TokenResponse>();
			//Console.WriteLine(token.Token);
		}

		[Test]
		public void Test1()
		{
			Assert.Pass();
		}
	}
}