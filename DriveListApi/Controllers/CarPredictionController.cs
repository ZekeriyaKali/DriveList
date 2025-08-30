using DriveListApi.Data;
using DriveListApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace DriveListApi.Controllers
{
    public class CarPredictionController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory; //socket sızıntısını azaltır, connection pooling yapar
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CarPredictionController(
            IHttpClientFactory httpClientFactory,
            AppDbContext context,
            UserManager<ApplicationUser> userManager
            )
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
            _userManager = userManager;
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

            // 🔹 Giriş yapmış kullanıcıyı al
            var user = await _userManager.GetUserAsync(User);

            // 🔹 DB’ye kaydet
            var history = new PredictionHistory
            {
                UserId = user?.Id, // Identity'nin string UserId'si
                Brand = vm.Brand,
                Model = vm.Model,
                Year = vm.Year,
                Km = vm.Km,
                GearType = vm.GearType,
                FuelType = vm.FuelType,
                City = vm.City,
                PredictedPrice = (decimal)vm.PredictedPrice
            };

            _context.PredictionHistories.Add(history);
            await _context.SaveChangesAsync();



            return View("Result", vm);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var history = await _context.PredictionHistories
                                        .Where(p => p.UserId == user.Id)
                                        .OrderByDescending(p => p.CreatedAt)
                                        .ToListAsync();

            return View(history);
        }
    }
}
