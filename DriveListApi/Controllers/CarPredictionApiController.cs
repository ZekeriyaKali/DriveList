using DriveListApi.Data;         // Veritabanı context'i (AppDbContext) için namespace
using DriveListApi.Models;       // CarRequest, Prediction, PredictionResponse gibi modeller için
using Microsoft.AspNetCore.Http; // HTTP ile ilgili sınıflar (IFormFile, StatusCodes vs.)
using Microsoft.AspNetCore.Mvc;  // ControllerBase, IActionResult vb. için gerekli namespace

namespace DriveListApi.Controllers
{
    // API route tanımı -> bu controller "/api/CarPredictionApi" adresinden erişilebilir
    [Route("api/[controller]")]
    [ApiController] // API davranışı: Model binding, otomatik 400 bad request, validation vb.
    public class CarPredictionApiController : ControllerBase
    {
        // HttpClientFactory -> HttpClient yönetimi (connection pooling, socket leak önleme)
        private readonly IHttpClientFactory _httpClientFactory;

        // EF Core DbContext -> MSSQL veritabanına erişim sağlar
        private readonly AppDbContext _db;

        // Constructor: bağımlılık enjeksiyonu ile HttpClientFactory ve DbContext alınır
        public CarPredictionApiController(IHttpClientFactory httpClientFactory, AppDbContext db)
        {
            _httpClientFactory = httpClientFactory;
            _db = db;
        }

        // API endpoint: /api/CarPredictionApi [POST]
        // İstemci JSON formatında CarRequest gönderir -> [FromBody] ile binding yapılır
        [HttpPost]
        public async Task<IActionResult> Predict([FromBody] CarRequest request)
        {
            // HttpClient oluştur -> Flask API’ye bağlanmak için kullanılacak
            var client = _httpClientFactory.CreateClient();

            // Flask API’ye CarRequest verisini JSON olarak POST et
            var flaskResponse = await client.PostAsJsonAsync("http://localhost:5000/predict", request);

            // Eğer Flask API hata dönerse (ör: 500, 404) -> 500 hatası ile geri dön
            if (!flaskResponse.IsSuccessStatusCode)
                return StatusCode(500, "Tahmin API hatası");

            // Flask API'nin JSON cevabını PredictionResponse objesine deserialize et
            var result = await flaskResponse.Content.ReadFromJsonAsync<PredictionResponse>();

            // Veritabanına kaydedilecek Prediction nesnesi oluştur
            var prediction = new Prediction
            {
                Brand = request.Brand,            
                Model = request.Model,          
                Year = request.Year,            
                Km = request.Km,                 
                GearType = request.GearType,      
                FuelType = request.FuelType,      
                City = request.City,              
                PredictedPrice = result.PricePrediction, // Flask API'den gelen tahmin sonucu
                CreatedAt = DateTime.Now          // Tahmin yapılan zaman
            };

            // EF Core ile MSSQL’e ekle
            _db.Predictions.Add(prediction);

            // Asenkron olarak DB’ye kaydet
            await _db.SaveChangesAsync();

            // Sonucu JSON formatında döndür (HTTP 200 + result)
            return Ok(result);
        }
    }
}
