using DriveListApi.Models;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace DriveListApi.Controllers
{
    public class CarPredictionController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory; //socket sızıntısını azaltır, connection pooling yapar

        public CarPredictionController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Create()  //Form sayfasını render eder
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CarRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("http://localhost:5000/predict", request); //flask api a json ile post etme

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Tahmin API hatası"); //Razor tarafında validation summary ile görüntülenir
                return View(request);
            }

            var prediction = await response.Content.ReadFromJsonAsync<PredictionResponse>(); // flask ın json cevabını deserialize eder 


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
