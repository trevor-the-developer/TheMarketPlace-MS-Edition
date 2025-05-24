using System.Reflection;
using AuthenticationService.Application.Contracts.Security;
using AuthenticationService.Application.Security;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace AuthenticationService.Application.Extensions;

public static class ApplicationServiceRegistrationExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Register security services
        services.AddScoped<ITokenService, TokenService>();
        
        return services;
    }
}