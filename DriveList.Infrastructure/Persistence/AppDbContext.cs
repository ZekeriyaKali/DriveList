using DriveList.Domain.Entities;
using DriveList.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DriveList.Infrastructure.Persistence
{
    public class AppDbContext:IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Prediction> Predictions { get; set; }
        public DbSet<PredictionHistory> PredictionHistories { get; set; }
        public DbSet<LoginAudit> LoginAudits { get; set; }
        public DbSet<PaymentHistory> PaymentHistories { get; set; }

    }
}
