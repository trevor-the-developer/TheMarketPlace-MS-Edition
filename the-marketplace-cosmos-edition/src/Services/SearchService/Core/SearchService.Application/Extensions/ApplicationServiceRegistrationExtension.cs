using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SearchService.Application.Extensions;

public static class ApplicationServiceRegistrationExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        return services;
    }
}