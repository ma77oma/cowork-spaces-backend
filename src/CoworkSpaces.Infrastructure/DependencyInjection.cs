using CoworkSpaces.Application.Common.Interfaces;
using CoworkSpaces.Domain.Services;
using CoworkSpaces.Infrastructure.Identity;
using CoworkSpaces.Infrastructure.Jobs;
using CoworkSpaces.Infrastructure.Persistence;
using CoworkSpaces.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace CoworkSpaces.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddQuartz(options =>
        {
            var jobKey = new JobKey(nameof(CompleteReservationsJob));

            options.AddJob<CompleteReservationsJob>(builder => builder.WithIdentity(jobKey));

            options.AddTrigger(builder => builder
                .ForJob(jobKey)
                .WithIdentity($"{nameof(CompleteReservationsJob)}-trigger")
                .WithCronSchedule("0 0/5 * * * ?"));
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<PricingService>();
        services.AddSingleton<CancellationPolicyService>();
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddHttpContextAccessor();

        return services;
    }
}
