using DriveList.Application.DTOs;
using DriveList.Domain.Entities;

namespace DriveList.Application.Services
{
    public interface ICarPredictionService
    {
        Task<PredictionResponse> PredictAsync(CarRequest request);
    }
}
