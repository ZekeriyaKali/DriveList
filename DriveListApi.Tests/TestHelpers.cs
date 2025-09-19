using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace DriveListApi.Tests
{
    public static class TestHelpers
    {
        // Sahte UserManager üretir
        public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            return new Mock<UserManager<TUser>>(
                store.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );
        }

        // Sahte SignInManager üretir
        public static Mock<SignInManager<TUser>> MockSignInManager<TUser>(
            Mock<UserManager<TUser>> userManager) where TUser : class
        {
            return new Mock<SignInManager<TUser>>(
                userManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<TUser>>().Object,
                null, null, null, null
            );
        }
    }
}
