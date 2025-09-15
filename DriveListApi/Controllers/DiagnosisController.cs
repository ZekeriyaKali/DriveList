using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.IO;

public class DiagnosisController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DiagnosisController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(string description, IFormFile? audio, IFormFile? image)
    {
        var client = _httpClientFactory.CreateClient("DiagnosisApi");

        using var form = new MultipartFormDataContent();

        if (!string.IsNullOrEmpty(description))
            form.Add(new StringContent(description), "description");

        if (audio != null)
        {
            await using var stream = audio.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(audio.ContentType);
            form.Add(fileContent, "audio", audio.FileName);
        }

        if (image != null)
        {
            await using var stream = image.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(image.ContentType);
            form.Add(fileContent, "image", image.FileName);
        }

        var response = await client.PostAsync("/diagnose", form);
        var json = await response.Content.ReadAsStringAsync();

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("text", out var textEl))
                ViewBag.TextResult = textEl.GetProperty("label").GetString();

            if (root.TryGetProperty("audio", out var audioEl) && audioEl.ValueKind != JsonValueKind.Null)
            {
                var label = audioEl.GetProperty("label").GetString();
                var advice = audioEl.GetProperty("advice").GetString();
                ViewBag.AudioResult = $"{label} - {advice}";
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
                ViewBag.ImageResult = list;
            }

            if (root.TryGetProperty("annotated_image", out var ann) && ann.ValueKind != JsonValueKind.Null)
                ViewBag.AnnotatedImage = ann.GetString();

            if (root.TryGetProperty("final_recommendations", out var finalRecos))
                ViewBag.FinalRecommendations = finalRecos.EnumerateArray().Select(x => x.GetString()).ToList();
        }
        catch (Exception ex)
        {
            ViewBag.Result = $"Sonuç alınamadı: {ex.Message}";
        }

        return View();
    }
}