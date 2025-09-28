using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Json;


namespace DriveListApi.Tests.Integration
{
    public class AccountFlowTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AccountFlowTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_Should_CreateUser()
        {
            var response = await _client.PostAsJsonAsync("/account/register", new
            {
                Username = "testuser",
                Email = "test@test.com",
                Password = "Test1234!"
            });

            response.EnsureSuccessStatusCode();
        }
    }
}
