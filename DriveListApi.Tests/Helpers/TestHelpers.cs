using DriveList.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace DriveListApi.Tests.Helpers
{
    public static class TestHelpers
    {
        // Sahte UserManager üretir
        public static ApplicationUser CreateTestUser(string username = "user")
        {
            return new ApplicationUser
            {
                UserName = username,
                Email = $"{username}@test.com",
                Credits = 10
            };
        }

        public static CarRequest CreateTestCarRequest()
        {
            return new CarRequest
            {
                Brand = "BMW",
                Model = "320i",
                Year = 2018,
                Km = 20000,
                FuelType = "Benzin",
                GearType = "Otomatik",
                City = "İstanbul"
            };
        }
    }
}
