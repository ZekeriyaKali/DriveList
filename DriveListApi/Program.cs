using DriveListApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddHttpClient();  // Flask API çaðrýsý için
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSession(); 

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=CarPrediction}/{action=Create}/{id?}"
);


app.MapControllers();

app.Run();
