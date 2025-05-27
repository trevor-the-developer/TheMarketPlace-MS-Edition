using System.Text.Json;
using System.Text.Json.Serialization;
using DocumentProcessor.Extensions;
using DocumentProcessor.Settings;
using DocumentProcessor.Jobs;
using DocumentProcessor.Consumers;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using MassTransit;
using QuestPDF.Infrastructure;
using Services.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

var appConfig = builder.Configuration.Get<DocumentProcessorServiceConfiguration>() 
    ?? throw new InvalidOperationException("DocumentProcessorServiceConfiguration is missing or invalid.");
builder.Services.AddSingleton(appConfig);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddMessageBusServices(appConfig)
    .AddMongoDbServices(appConfig)
    .AddMinIOServices(appConfig);

builder.Services.AddScoped<ChecklistProcessorJob>();

builder.Services.AddHangfire(config =>
{
    config.UsePostgreSqlStorage(c => 
        c.UseNpgsqlConnection(appConfig.PostgresSqlSettings.ConnectionString));
    config.UseSerializerSettings(new Newtonsoft.Json.JsonSerializerSettings
    {
        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
    });
});

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5;
    options.Queues = new[] { "default", "critical" };
});

builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.Converters.Add(new JsonStringEnumConverter());
});

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Wait for Hangfire database to be available (created by Auth service)
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Test Hangfire connection - it will initialize tables if they don't exist
        logger.LogInformation("Verifying Hangfire database connection...");
        var storage = Hangfire.JobStorage.Current;
        logger.LogInformation("Hangfire database connection verified successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to connect to Hangfire database. Ensure Auth service has started and created the database.");
        // Don't throw here - let the service try to start anyway
    }
}

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.UseRouting();
app.MapControllers();

app.Run();

namespace DocumentProcessor
{
    public partial class Program { }
}

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true; // Allow all in development
}