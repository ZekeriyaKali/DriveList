using DriveListApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DriveListApi.Controllers
{
    public class CarPredictionController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CarPredictionController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CarRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("http://localhost:5000/predict", request);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Tahmin API hatası");
                return View(request);
            }

            var prediction = await response.Content.ReadFromJsonAsync<PredictionResponse>();


            var vm = new PredictionViewModel
            {
                Brand = request.Brand,
                Model = request.Model,
                Year = request.Year,
                Km = request.Km,
                GearType = request.GearType,
                FuelType = request.FuelType,
                City = request.City,
                PredictedPrice = prediction.PricePrediction
            };


            return View("Result", vm);
        }
    }
}
