using Microsoft.AspNetCore.Mvc;               // MVC Controller ve IActionResult için gerekli namespace
using System.Net.Http.Headers;               // HTTP içerik header’larını ayarlamak için
using System.Text.Json;                      // JSON parse işlemleri için
using System.IO;                             // Stream işlemleri için (dosya okuma/yazma)

// Kullanıcının formdan girdiği açıklama, ses ve görüntü dosyalarını API’ye gönderip
// gelen teşhis (diagnosis) sonuçlarını view tarafında göstermek için kullanılan controller
public class DiagnosisController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory; // HTTP istekleri yapmak için dependency injection ile alınan factory

    // Constructor: IHttpClientFactory injection edilir
    public DiagnosisController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory; // Bağımlılığı sınıf seviyesinde saklıyoruz
    }

    // GET: /Diagnosis/Create
    // Kullanıcıya boş formu göstermek için çağrılır
    [HttpGet]
    public IActionResult Create() => View();

    // POST: /Diagnosis/Create
    // Kullanıcı formu gönderdiğinde (description, audio, image) buraya gelir
    [HttpPost]
    public async Task<IActionResult> Create(string description, IFormFile? audio, IFormFile? image)
    {
        // API client’ı IHttpClientFactory üzerinden alıyoruz (Startup/Program.cs’de tanımlı "DiagnosisApi" client)
        var client = _httpClientFactory.CreateClient("DiagnosisApi");

        // Form verilerini API’ye göndermek için MultipartFormDataContent oluşturuyoruz
        using var form = new MultipartFormDataContent();

        // Eğer kullanıcı açıklama (description) girdiyse form’a ekle
        if (!string.IsNullOrEmpty(description))
            form.Add(new StringContent(description), "description");

        // Eğer ses dosyası yüklendiyse
        if (audio != null)
        {
            // Dosyayı stream olarak aç
            await using var stream = audio.OpenReadStream();

            // StreamContent ile HTTP request gövdesine koymak için sarmala
            var fileContent = new StreamContent(stream);

            // Content-Type header’ını dosyanın MIME type’ına göre ayarla
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(audio.ContentType);

            // Form’a ekle ("audio" alanı, dosya adı ile birlikte)
            form.Add(fileContent, "audio", audio.FileName);
        }

        // Eğer resim dosyası yüklendiyse
        if (image != null)
        {
            // Dosyayı stream olarak aç
            await using var stream = image.OpenReadStream();

            // StreamContent ile request’e ekle
            var fileContent = new StreamContent(stream);

            // MIME type ayarı
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(image.ContentType);

            // Form’a ekle ("image" alanı, dosya adı ile birlikte)
            form.Add(fileContent, "image", image.FileName);
        }

        // API’ye POST isteği gönder (/diagnose endpoint’i)
        var response = await client.PostAsync("/diagnose", form);

        // Dönen JSON yanıtını string olarak al
        var json = await response.Content.ReadAsStringAsync();

        try
        {
            // JSON parse etmek için JsonDocument kullan
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Eğer text sonucu geldiyse al ve ViewBag’e koy
            if (root.TryGetProperty("text", out var textEl))
                ViewBag.TextResult = textEl.GetProperty("label").GetString();

            // Eğer ses analizi sonucu geldiyse label ve advice’ı oku
            if (root.TryGetProperty("audio", out var audioEl) && audioEl.ValueKind != JsonValueKind.Null)
            {
                var label = audioEl.GetProperty("label").GetString();
                var advice = audioEl.GetProperty("advice").GetString();
                ViewBag.AudioResult = $"{label} - {advice}";
            }

            // Eğer resim analizi sonucu geldiyse (detected objects)
            if (root.TryGetProperty("image", out var imageEl) && imageEl.ValueKind != JsonValueKind.Null)
            {
                var dets = imageEl.GetProperty("detections");
                var list = new List<object>();

                // Tespit edilen her objeyi listeye ekle
                foreach (var d in dets.EnumerateArray())
                {
                    list.Add(new
                    {
                        label = d.GetProperty("label").GetString(),
                        title = d.GetProperty("title").GetString(),
                        advice = d.GetProperty("advice").GetString()
                    });
                }

                // ViewBag’e set et (view’da foreach ile gösterilebilir)
                ViewBag.ImageResult = list;
            }

            // Eğer annotate edilmiş (işaretlenmiş) resim döndüyse al
            if (root.TryGetProperty("annotated_image", out var ann) && ann.ValueKind != JsonValueKind.Null)
                ViewBag.AnnotatedImage = ann.GetString();

            // Eğer final öneriler listesi varsa listeye çevirip ViewBag’e ata
            if (root.TryGetProperty("final_recommendations", out var finalRecos))
                ViewBag.FinalRecommendations = finalRecos.EnumerateArray().Select(x => x.GetString()).ToList();
        }
        catch (Exception ex)
        {
            // JSON parse ya da başka bir hata olursa kullanıcıya mesaj göster
            ViewBag.Result = $"Sonuç alınamadı: {ex.Message}";
        }

        // View’a dön (sonuçlar ViewBag ile taşınacak)
        return View();
    }
}
