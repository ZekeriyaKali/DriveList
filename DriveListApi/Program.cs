using DriveListApi.Data;
using DriveListApi.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // ✅ E-posta onayı zorunlu
    options.SignIn.RequireConfirmedAccount = true;

    // 🔐 Parola politikası (istersen burada daha da sıkılaştırabilirsin)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;          // 6 → 8 önerisi
    options.Password.RequireNonAlphanumeric = false;

    // 🛡️ Lockout ayarları (yanlış şifre denemesinde kilitle)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

/*builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.CallbackPath = "/signin-google";

        options.Events.OnRemoteFailure = context =>
        {
            context.Response.Redirect("/Account/Login?error=access_denied");
            context.HandleResponse();
            return Task.CompletedTask;
        };
    });*/

/*builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Google";
})
.AddCookie()
.AddOAuth("Google", options =>
{
    options.ClientId = builder.Configuration["GoogleKeys:ClientId"];
    options.ClientSecret = builder.Configuration["GoogleKeys:ClientSecret"];
    options.CallbackPath = new PathString("/signin-google");

    options.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    options.TokenEndpoint = "https://oauth2.googleapis.com/token";
    options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";

    options.Scope.Add("https://www.googleapis.com/auth/userinfo.email");
    options.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");

    options.Events.OnRemoteFailure = context =>
    {
        context.Response.Redirect("/Account/Login?error=access_denied");
        context.HandleResponse();
        return Task.CompletedTask;
    };
});*/


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
options.DefaultChallengeScheme = "Google";
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google"; // default zaten bu
    options.Events.OnRemoteFailure = context =>
    {
        context.Response.Redirect("/Account/Login?error=access_denied");
        context.HandleResponse();
        return Task.CompletedTask;
    };
});


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});


builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddHttpClient();  // Flask API çağrısı için
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
// (İsteğe bağlı) Session — kısa ömürlü UI verileri için
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(20);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication();    // Identity için şart
app.UseAuthorization();
app.UseSession();           // Session kullanacaksan

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Default Identity UI endpoint’leri (Login/Register) için:
app.MapRazorPages();

app.MapControllers();

app.Run();
