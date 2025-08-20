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

            ViewBag.Brand = request.Brand;
            ViewBag.Model = request.Model;
            ViewBag.Year = request.Year;
            ViewBag.Km = request.Km;
            ViewBag.GearType = request.GearType;
            ViewBag.FuelType = request.FuelType;
            ViewBag.City = request.City;
            ViewBag.PredictedPrice = prediction.PricePrediction;

            return View("Result", prediction);
        }
    }
}
