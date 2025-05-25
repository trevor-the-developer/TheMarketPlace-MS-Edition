using SearchService.Application.Extensions;
using SearchService.Application.Settings;
using SearchService.Persistence.Extensions;
using Services.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);
var appConfig = builder.Services.AddApplicationConfiguration<SearchServiceConfiguration>(builder.Configuration);

builder.Services.AddSingleton(appConfig);
builder.Services.AddControllers();

builder.Services
    .AddApplicationServices()
    .AddMessageBusServices(appConfig)
    .AddPersistenceServices(appConfig)
    .AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    })
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.UseCors();

app.Run();

namespace SearchService.Api
{
    public partial class Program { }
}