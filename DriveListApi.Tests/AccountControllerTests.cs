using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using DriveListApi.Controllers;
using DriveListApi.Models;
using DriveListApi.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace DriveListApi.Tests
{
    public class AccountControllerTests
    {
        private AccountController GetController(
        Mock<UserManager<ApplicationUser>> userManagerMock,
        Mock<SignInManager<ApplicationUser>> signInManagerMock,
        Mock<IConfiguration> configMock,
        Mock<IHttpClientFactory> httpClientFactoryMock)
        {
            var dbContext = new Mock<AppDbContext>();
            return new AccountController(
                userManagerMock.Object,
                signInManagerMock.Object,
                dbContext.Object,
                configMock.Object,
                httpClientFactoryMock.Object
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task Register_InvalidRecaptcha_ReturnsViewWithError()
        {
            // Arrange
            var userManagerMock = TestHelpers.MockUserManager<ApplicationUser>();
            var signInManagerMock = TestHelpers.MockSignInManager<ApplicationUser>(userManagerMock);
            var configMock = new Mock<IConfiguration>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();

            var controller = GetController(userManagerMock, signInManagerMock, configMock, httpClientFactoryMock);

            // reCAPTCHA sahte yanıt
            controller.ControllerContext.HttpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "g-recaptcha-response", "" }
        });

            var model = new RegisterViewModel { Username = "test", Email = "test@test.com", Password = "Pass123!" };

            // Act
            var result = await controller.Register(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }
    }
}
