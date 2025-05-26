using AuthenticationService.Application.Settings;
using AuthenticationService.Domain.Entities;
using AuthenticationService.Persistence.DatabaseContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace AuthenticationService.Persistence.Extensions;

public static class PersistenceServiceRegistrationExtension
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, AuthenticationServiceConfiguration configuration)
    {
        services.AddDbContext<AuthenticationDbContext>(options =>
            options.UseNpgsql(configuration.ConnectionString));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedEmail = true;
                
                // Add explicit token provider configuration
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Tokens.ProviderMap.Add("Default", new TokenProviderDescriptor(
                    typeof(EmailTokenProvider<ApplicationUser>)));
            })
            .AddEntityFrameworkStores<AuthenticationDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}