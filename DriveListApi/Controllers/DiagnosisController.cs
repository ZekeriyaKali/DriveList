using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.IO;

public class DiagnosisController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebHostEnvironment _env;

    public DiagnosisController(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
    {
        _httpClientFactory = httpClientFactory;
        _env = env;
    }

    [HttpGet]
    public IActionResult Create() => View();

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

        var response = await client.PostAsync("http://localhost:5001/diagnose", form);
        var json = await response.Content.ReadAsStringAsync();

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            ViewBag.TextResult = root.GetProperty("text").GetProperty("label").GetString();
            if (root.TryGetProperty("audio", out var audioEl) && audioEl.ValueKind != JsonValueKind.Null)
            {
                ViewBag.AudioLabel = audioEl.GetProperty("label").GetString();
                ViewBag.AudioAdvice = audioEl.GetProperty("advice").GetString();
                ViewBag.AudioFile = audioEl.GetProperty("file").GetString();
            }

            if (root.TryGetProperty("image", out var imageEl) && imageEl.ValueKind != JsonValueKind.Null)
            {
                var dets = imageEl.GetProperty("detections");
                var list = new List<object>();
                foreach (var d in dets.EnumerateArray())
                {
                    list.Add(new
                    {
                        label = d.GetProperty("label").GetString(),
                        title = d.GetProperty("title").GetString(),
                        advice = d.GetProperty("advice").GetString()
                    });
                }
                ViewBag.ImageDetections = list;
            }

            if (root.TryGetProperty("annotated_image", out var ann) && ann.ValueKind != JsonValueKind.Null)
            {
                var annotatedPath = ann.GetString();
                // annotatedPath is server-side path on Flask host; if same host, you can serve static or copy to wwwroot.
                // For simplicity, just pass the path to view
                ViewBag.AnnotatedImage = annotatedPath;
            }

            // final recos
            var finalRecos = root.GetProperty("final_recommendations");
            ViewBag.FinalRecommendations = finalRecos.EnumerateArray().Select(x => x.GetString()).ToList();
        }
        catch
        {
            ViewBag.Result = "Sonuç alınamadı veya geçersiz formatta geldi.";
        }

        return View();
    }
}