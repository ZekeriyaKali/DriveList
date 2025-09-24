using DriveList.Domain.Entities;
using DriveList.Application.Common.Interfaces;
using DriveList.Application
using System.Net.Http.Json;


namespace DriveList.Application.Services
{
    public class CarPredictionService : ICarPredictionService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppDbContext _context;

        public CarPredictionService(IHttpClientFactory httpClientFactory, AppDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        public async Task<PredictionResponse> PredictAsync(CarRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            var flaskResponse = await client.PostAsJsonAsync("http://localhost:5000/predict", request);

            if (flaskResponse.IsSuccessStatusCode)
            {
                throw new Exception("Tahmin API hatası");
            }

            var result = await flaskResponse.Content.ReadFromJsonAsync<PredictionResponse>();

            var prediction = new Prediction
            {
                Brand = request.Brand,
                Model = request.Model,
                Year = request.Year,
                Km = request.Km,
                GearType = request.GearType,
                FuelType = request.FuelType,
                City = request.City,
                PredictedPrice = result.PricePrediction,
                CreatedAt = DateTime.Now
            };

            _context.Predictions.Add(prediction);
            await _context.SaveChangesAsync();

            return result!;

        }
    }
}
