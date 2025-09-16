using DriveListApi.Data;
using DriveListApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveListApi.Controllers
{
    public class CarPredictionController : Controller
    {
        // IHttpClientFactory -> socket sızıntısını engeller, connection pooling yapar, API çağrılarında kullanılır
        private readonly IHttpClientFactory _httpClientFactory;

        // Veritabanı bağlamı (EF Core DbContext)
        private readonly AppDbContext _context;

        // ASP.NET Identity User Manager (kullanıcı yönetimi için)
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor DI -> controller için bağımlılık enjeksiyonu
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
        public IActionResult Create()  // Formu render eder (kullanıcıya boş form döner)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CarRequest request)
        {
            // Flask API ile iletişim kurmak için HttpClient oluştur
            var client = _httpClientFactory.CreateClient();

            // Flask API'ye tahmin isteğini JSON formatında POST et
            var response = await client.PostAsJsonAsync("http://localhost:5000/predict", request);

            // Eğer API hatalı dönerse kullanıcıya hata mesajı gönder
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Tahmin API hatası");
                return View(request); // formu tekrar göster
            }

            // Flask API'nin JSON cevabını PredictionResponse modeline deserialize et
            var prediction = await response.Content.ReadFromJsonAsync<PredictionResponse>();

            // ViewModel doldur -> Razor sayfasına gönderilecek tahmin sonucu
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

            // Giriş yapmış kullanıcıyı al
            var user = await _userManager.GetUserAsync(User);

            // Kullanıcının kredisi yoksa tahmin yapılamaz
            if (user.Credits <= 0)
            {
                ModelState.AddModelError("", "Yeterli krediniz yok.");
                return View(request);
            }

            // Kullanıcıdan 1 kredi düş
            user.Credits -= 1;
            await _userManager.UpdateAsync(user);

            // Tahmin geçmişini veritabanına kaydet
            var history = new PredictionHistory
            {
                UserId = user?.Id, // Identity kullanıcısının Id’si
                Brand = vm.Brand,
                Model = vm.Model,
                Year = vm.Year,
                Km = vm.Km,
                GearType = vm.GearType,
                FuelType = vm.FuelType,
                City = vm.City,
                PredictedPrice = (decimal)vm.PredictedPrice
            };

            // DB’ye ekle ve kaydet
            _context.PredictionHistories.Add(history);
            await _context.SaveChangesAsync();

            // Sonuç sayfasına yönlendir (Result.cshtml)
            return View("Result", vm);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kullanıcının geçmiş tahminlerini getir, en son yapılanı en üstte olacak şekilde sırala
            var history = await _context.PredictionHistories
                                        .Where(p => p.UserId == user.Id)
                                        .OrderByDescending(p => p.CreatedAt)
                                        .ToListAsync();

            // Tahmin geçmişini View’a gönder
            return View(history);
        }
    }
}
