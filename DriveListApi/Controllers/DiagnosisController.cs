using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DriveListApi.Controllers
{
    public class DiagnosisController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DiagnosisController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string description, IFormFile? audio, IFormFile? image)
        {
            var client = _httpClientFactory.CreateClient();

            using var form = new MultipartFormDataContent();
            if (!string.IsNullOrEmpty(description))
                form.Add(new StringContent(description), "description");

            if (audio != null)
            {
                var stream = audio.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(audio.ContentType);
                form.Add(fileContent, "audio", audio.FileName);
            }

            if (image != null)
            {
                var stream = image.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(image.ContentType);
                form.Add(fileContent, "image", image.FileName);
            }

            // Flask servisine istek atıyoruz
            var response = await client.PostAsync("http://localhost:5001/diagnose", form);
            var json = await response.Content.ReadAsStringAsync();

            try
            {
                // JSON'dan sadece "diagnosis" alanını al
                var diagnosis = JsonDocument.Parse(json)
                                          .RootElement
                                          .GetProperty("diagnosis")
                                          .GetString();
                ViewBag.Result = diagnosis;
            }
            catch
            {
                ViewBag.Result = "Sonuç alınamadı veya geçersiz formatta geldi.";
            }
            return View();
        }
    }
}
