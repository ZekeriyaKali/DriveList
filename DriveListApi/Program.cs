using DriveList.Application.Services;
using DriveListApi.Data;                       // Veritabanı context sınıfını (AppDbContext) eklemek için
using DriveListApi.Models;                     // ApplicationUser gibi Identity modellerini eklemek için
using Microsoft.AspNetCore.Authentication.Cookies; // Cookie tabanlı kimlik doğrulama için
using Microsoft.AspNetCore.Identity;           // ASP.NET Identity sistemi (kullanıcı, roller) için
using Microsoft.EntityFrameworkCore;           // EF Core (DbContext, LINQ, migration) için
using System.Threading.RateLimiting;           // Rate limiting (istek sınırlandırma) middleware’i için

// -----------------------------------------------------------
// Web uygulaması builder’ı (DI container, config ve logging dahil)
// -----------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------
// 1) Database Configuration (EF Core + SQL Server)
// -----------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// AppDbContext SQL Server kullanacak, bağlantı bilgisi appsettings.json -> "DefaultConnection"

// -----------------------------------------------------------
// 2) Identity Configuration (Kullanıcı yönetimi + roller)
// -----------------------------------------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // ✅ E-posta onayı olmadan giriş yapılamaz
    options.SignIn.RequireConfirmedAccount = true;

    // 🔐 Parola politikası
    options.Password.RequireDigit = true;            // en az bir rakam
    options.Password.RequireLowercase = true;        // küçük harf zorunlu
    options.Password.RequireUppercase = true;        // büyük harf zorunlu
    options.Password.RequiredLength = 8;             // minimum 8 karakter
    options.Password.RequireNonAlphanumeric = false; // özel karakter zorunlu değil

    // 🛡️ Lockout (hesap kitlenmesi)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // 5 dakika kitlenir
    options.Lockout.MaxFailedAccessAttempts = 5; // 5 yanlış denemeden sonra kitlenir
    options.Lockout.AllowedForNewUsers = true;   // yeni kullanıcılar için de geçerli
})
.AddEntityFrameworkStores<AppDbContext>() // Identity kullanıcıları EF Core DB’de saklanır
.AddDefaultTokenProviders();              // E-posta doğrulama, şifre sıfırlama token sağlayıcıları

// -----------------------------------------------------------
// 3) Authentication Configuration (Cookie + Google OAuth2)
// -----------------------------------------------------------

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Default login yöntemi cookie
    options.DefaultChallengeScheme = "Google"; // Challenge (zorunlu yönlendirme) -> Google OAuth
})
.AddCookie() // Cookie tabanlı kimlik doğrulama aktif
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];     // Google ClientId
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]; // Google ClientSecret
    options.CallbackPath = "/signin-google"; // Google’dan dönüş URL’si (default bu)

    // Google login başarısız olursa login sayfasına yönlendir
    options.Events.OnRemoteFailure = context =>
    {
        context.Response.Redirect("/Account/Login?error=access_denied");
        context.HandleResponse();
        return Task.CompletedTask;
    };
});

// -----------------------------------------------------------
// 4) Rate Limiting (DDOS/Brute force koruması)
// -----------------------------------------------------------
builder.Services.AddRateLimiter(options =>
{
    // "loginPolicy" adında rate limit politikası ekleniyor
    options.AddPolicy("loginPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon", // kullanıcı IP adresi
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,                  // 1 dakika içinde max 10 istek
                Window = TimeSpan.FromMinutes(1),  // sabit pencere: 1 dakika
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst, // en eski istek önce işlenir
                QueueLimit = 0                     // fazla istek kuyruklanmaz, direkt reddedilir
            }));

    // Limit aşılırsa verilecek cevap
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests; // HTTP 429
        await context.HttpContext.Response.WriteAsync("Çok fazla istek yaptınız. Lütfen biraz bekleyin.", token);
    };
});

// -----------------------------------------------------------
// 5) Cookie Security Hardening
// -----------------------------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";           // login olmayanlar buraya yönlendirilir
    options.LogoutPath = "/Account/Logout";         // logout endpoint
    options.AccessDeniedPath = "/Account/AccessDenied"; // yetki yoksa buraya

    // 🍪 Cookie güvenliği
    options.Cookie.Name = "DriveList.Auth";         // cookie adı
    options.Cookie.HttpOnly = true;                 // JS erişimi kapalı
    options.Cookie.SameSite = SameSiteMode.Strict;  // CSRF önleme
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // sadece HTTPS üzerinden gönder
    options.ExpireTimeSpan = TimeSpan.FromHours(8); // 8 saat oturum
    options.SlidingExpiration = true;               // süre dolmadan yenilenir
});

// -----------------------------------------------------------
// 6) MVC + Razor + HttpClient + Session Servisleri
// -----------------------------------------------------------
builder.Services.AddRazorPages().AddRazorRuntimeCompilation(); // Razor hot reload
builder.Services.AddHttpClient();  // Flask API çağrısı için HttpClient
builder.Services.AddControllers(); // API controller desteği
builder.Services.AddControllersWithViews(); // MVC controller + view desteği

// (opsiyonel) Session: kısa süreli veriler için
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(20); // 20 dk session süresi
    o.Cookie.HttpOnly = true;                 // JS erişemez
    o.Cookie.IsEssential = true;              // GDPR için zorunlu cookie
});

// -----------------------------------------------------------
// 7) Middleware Pipeline
// -----------------------------------------------------------
var app = builder.Build();

app.UseStaticFiles();     // wwwroot altındaki statik dosyaları sunar (css/js/img)
app.UseRateLimiter();     // Rate limiting middleware aktif
app.UseRouting();         // Endpoint routing

app.UseHttpsRedirection(); // HTTP → HTTPS yönlendirmesi

app.UseAuthentication();  // kullanıcı kimlik doğrulama
app.UseAuthorization();   // rol / policy kontrolü
app.UseSession();         // Session middleware

// -----------------------------------------------------------
// 8) Endpoint Tanımları
// -----------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// Varsayılan route -> HomeController.Index

app.MapRazorPages();  // Identity’nin hazır Razor Pages endpointleri (Login/Register)
app.MapControllers(); // API Controller endpointleri (örn. /api/CarPredictionApi)

// -----------------------------------------------------------
// 9) Uygulama Çalıştırma
// -----------------------------------------------------------
app.Run(); // Uygulamayı çalıştırır
