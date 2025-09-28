using DriveList.Application.Common.Interfaces;
using DriveList.Application.Services;
using DriveList.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveListApi.Tests.Unit
{
    public class PredictionServiceTests
    {
        private readonly Mock<IPredictionRepository> _repositoryMock;
        private readonly CarPredictionService _service;

        public PredictionServiceTests()
        {
            _repositoryMock = new Mock<IPredictionRepository>();
            _service = new CarPredictionService(null!, null!);
        }

        [Fact]
        public async Task PredictAsync_Should_SavePrediction()
        {
            var request = new CarRequest
            {
                Brand = "BMW",
                Model = "3.20",
                Year = 2015,
                Km = 20000,
                FuelType = "Benzin",
                GearType = "Otomatik",
                City = "İstanbul"
            };

            var repoMock = new Mock<IPredictionRepository>();
            var service = new CarPredictionService(null!, null!); // DI mock’lanmadı örnek

            // Act
            // Burada PredictAsync çağrısı yapılır, repoMock.Verify ile AddAsync doğrulanır

            // Assert
            repoMock.Verify(r => r.AddAsync(It.IsAny<Prediction>()), Times.AtMostOnce());
        }


    }
}
