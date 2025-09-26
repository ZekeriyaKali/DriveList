using DriveList.Application.Common.Interfaces;
using DriveList.Infrastructure.Identity;
using DriveList.Infrastructure.Persistence;
using DriveList.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DriveList.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options=>
            {
               options.SignIn.RequireConfirmedAccount = true;
               options.Password.RequireDigit = true;
               options.Password.RequireLowercase = true;
               options.Password.RequiredLength = 8;
               options.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IPredictionRepository, PredictionRepository>();

            return services;
        }

    }
}
