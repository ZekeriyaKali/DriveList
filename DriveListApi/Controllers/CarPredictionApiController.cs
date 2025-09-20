using DriveList.Application.Services;
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
        private readonly ICarPredictionService _carPredictionService;

        public CarPredictionApiController(ICarPredictionService carPredictionService)
        {
            _carPredictionService = carPredictionService;
        }

        [HttpPost]
        public async Task<IActionResult> Predict([FromBody] CarRequest request)
        {
            var result = await _carPredictionService.PredictAsync(request);
            return Ok(result);
        }
    }

    }

