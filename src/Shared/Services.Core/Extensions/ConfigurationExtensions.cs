using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Services.Core.Extensions;

public static class ConfigurationExtensions
{
    public static T AddApplicationConfiguration<T>(
        this IServiceCollection services,
        IConfiguration configuration) where T : class, IApplicationConfiguration
    {
        var applicationConfig = configuration.GetSection(nameof(ApplicationConfiguration))
            .Get<T>(options =>
            {
                options.ErrorOnUnknownConfiguration = true;
                options.BindNonPublicProperties = false;
            }) ?? throw new InvalidOperationException($"Configuration section '{nameof(ApplicationConfiguration)}' is missing or invalid.");

        services.AddOptions<T>()
            .Bind(configuration.GetSection(nameof(ApplicationConfiguration)));
        
        services.AddSingleton<IApplicationConfiguration>(applicationConfig);

        return applicationConfig;
    }
}
