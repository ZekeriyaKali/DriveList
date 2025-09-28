using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DriveListApi.Tests
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
