using DriveListApi.Data;
using DriveListApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DriveListApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppDbContext _db;

        public PredictionController(IHttpClientFactory httpClientFactory, AppDbContext db)
        {
            _httpClientFactory = httpClientFactory;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Predict([FromBody] CarRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            var flaskResponse = await client.PostAsJsonAsync("http://localhost:5000/predict", request);
            if (!flaskResponse.IsSuccessStatusCode)
                return StatusCode(500, "Tahmin API hatası");

            var result = await flaskResponse.Content.ReadFromJsonAsync<PredictionResponse>();

            // MSSQL'e kaydet
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
            _db.Predictions.Add(prediction);
            await _db.SaveChangesAsync();

            return Ok(result);
        }
    }
}
