using DriveListApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveList.Application.Services
{
    public interface ICarPredictionService
    {
        Task<PredictionResponse> PredictAsync(CarRequest request);
    }
}
