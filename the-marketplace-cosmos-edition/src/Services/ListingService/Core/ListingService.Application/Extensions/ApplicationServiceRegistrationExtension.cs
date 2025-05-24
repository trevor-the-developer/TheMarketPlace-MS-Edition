using System.Reflection;
using FluentValidation;
using ListingService.Application.Services.CurrentUserService;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ListingService.Application.Extensions;

public static class ApplicationServiceRegistrationExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Register HttpContextAccessor
        services.AddHttpContextAccessor();
        
        // Register CurrentUserService
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        return services;
    }
}