using DriveListApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DriveListApi.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Prediction> Predictions { get; set; }
    }
}
