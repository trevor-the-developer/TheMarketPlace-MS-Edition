using AuthenticationService.Application.Settings;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Persistence.DatabaseContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationService.Persistence.Extensions;

public static class PersistenceServiceRegistrationExtension
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, AuthenticationServiceConfiguration configuration)
    {
        services.AddDbContext<AuthenticationDbContext>(options =>
            options.UseSqlServer(configuration.ConnectionString));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<AuthenticationDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}