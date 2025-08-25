using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

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

            ViewBag.Result = json;
            return View();
        }
    }
}
