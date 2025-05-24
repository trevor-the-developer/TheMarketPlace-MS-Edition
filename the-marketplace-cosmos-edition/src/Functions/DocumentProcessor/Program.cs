using System.Text.Json;
using System.Text.Json.Serialization;
using DocumentProcessor.Extensions;
using DocumentProcessor.Settings;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestPDF.Infrastructure;
using Services.Core.Extensions;

var builder = FunctionsApplication.CreateBuilder(args);
var appConfig = builder.Services.AddApplicationConfiguration<DocumentProcessorServiceConfiguration>(builder.Configuration);

builder.Services.AddSingleton(appConfig);

builder.Services
    .AddMessageBusServices(appConfig)
    .AddCosmosDbServices(appConfig)
    .AddBlobStorageServices(appConfig)
    .Configure<JsonSerializerOptions>(options =>
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.Converters.Add(new JsonStringEnumConverter());
    });

QuestPDF.Settings.License = LicenseType.Community;

builder.ConfigureFunctionsWebApplication();
builder.Build().Run();