using DriveListApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace DriveListApi.Data
{
    public class AppDbContext:IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Prediction> Predictions { get; set; }
        public DbSet<PredictionHistory> PredictionHistories { get; set; }
    }
}
