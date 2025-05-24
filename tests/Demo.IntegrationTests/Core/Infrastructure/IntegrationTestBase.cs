using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.MongoDb;
using Testcontainers.Minio;
using Xunit;

namespace Demo.IntegrationTests.Core.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly PostgreSqlContainer PostgreSqlContainer;
    protected readonly RabbitMqContainer RabbitMqContainer;
    protected readonly MongoDbContainer MongoDbContainer;
    protected readonly MinioContainer MinioContainer;
    
    protected string PostgreSqlConnectionString => PostgreSqlContainer.GetConnectionString();
    protected string RabbitMqConnectionString => RabbitMqContainer.GetConnectionString();
    protected string MongoDbConnectionString => MongoDbContainer.GetConnectionString();
    protected string MinioEndpoint => $"{MinioContainer.Hostname}:{MinioContainer.GetMappedPublicPort(9000)}";
    
    protected IntegrationTestBase()
    {
        PostgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();

        RabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .WithCleanUp(true)
            .Build();

        MongoDbContainer = new MongoDbBuilder()
            .WithImage("mongo:latest")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();

        MinioContainer = new MinioBuilder()
            .WithImage("minio/minio")
            .WithUsername("minioadmin")
            .WithPassword("minioadmin")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            PostgreSqlContainer.StartAsync(),
            RabbitMqContainer.StartAsync(),
            MongoDbContainer.StartAsync(),
            MinioContainer.StartAsync()
        );

        await Task.Delay(2000); // Wait for containers to be fully ready
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            PostgreSqlContainer.StopAsync(),
            RabbitMqContainer.StopAsync(),
            MongoDbContainer.StopAsync(),
            MinioContainer.StopAsync()
        );
    }

    protected IServiceCollection CreateTestServices()
    {
        var services = new ServiceCollection();
        
        services.AddLogging(builder => builder.AddConsole());
        
        return services;
    }
}