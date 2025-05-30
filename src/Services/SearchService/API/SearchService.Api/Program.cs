using SearchService.Application.Extensions;
using SearchService.Application.Settings;
using SearchService.Persistence.Extensions;
using Services.Core.Extensions;

#region Setup and init the web application builder

var builder = WebApplication.CreateBuilder(args);
var appConfig = builder.Services.AddApplicationConfiguration<SearchServiceConfiguration>(builder.Configuration);

#endregion

#region Add services to the container

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

#endregion

var app = builder.Build();

#region Setup the web application

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.UseCors();

#endregion

app.Run();

namespace SearchService.Api
{
    public partial class Program { }
}