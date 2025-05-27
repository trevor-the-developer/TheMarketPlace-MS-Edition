using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AuthenticationService.Application.Extensions;
using AuthenticationService.Application.Settings;
using AuthenticationService.Persistence.DatabaseContext;
using AuthenticationService.Persistence.Extensions;
using AuthenticationService.Persistence.SeedData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Hangfire;
using Hangfire.PostgreSql;
using Services.Core.DatabaseContext;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure settings
var authServiceConfiguration = builder.Configuration
    .GetSection("AuthenticationService")
    .Get<AuthenticationServiceConfiguration>() 
    ?? throw new InvalidOperationException("AuthenticationService configuration section is missing or invalid");

// Now authServiceConfiguration is guaranteed to be non-null
builder.Services.AddSingleton(authServiceConfiguration);

// Application services
builder.Services.AddApplicationServices();

// Persistence services
builder.Services.AddPersistenceServices(authServiceConfiguration);

// Add Hangfire DbContext for database creation
builder.Services.AddDbContext<HangfireDbContext>(options =>
    options.UseNpgsql($"Server=postgres:5432;Database=hangfire;User Id=postgres;Password=postgrespw;"));

// Add Hangfire services (minimal setup for database creation only)
builder.Services.AddHangfire(config =>
{
    config.UsePostgreSqlStorage(c => 
        c.UseNpgsqlConnection("Server=postgres:5432;Database=hangfire;User Id=postgres;Password=postgrespw;"));
});

// JWT authentication
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authServiceConfiguration.JwtSettings.Key)),
        ValidateIssuer = true,
        ValidIssuer = authServiceConfiguration.JwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = authServiceConfiguration.JwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = "sub",
        RoleClaimType = "role"
    };
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Authentication Service API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Apply migrations for both Auth and Hangfire databases
try
{
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        // Auth database migration
        var authContext = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        logger.LogInformation("Applying authentication database migrations...");
        authContext.Database.Migrate();
        logger.LogInformation("Authentication database migrations applied successfully.");
        
        // Hangfire database migration
        var hangfireContext = scope.ServiceProvider.GetRequiredService<HangfireDbContext>();
        logger.LogInformation("Applying Hangfire database migrations...");
        hangfireContext.Database.Migrate();
        logger.LogInformation("Hangfire database migrations applied successfully.");
        
        // Seed roles
        logger.LogInformation("Seeding role data...");
        await SeedRoleData.SeedRolesAsync(roleManager, logger);
        logger.LogInformation("Role seeding completed.");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while applying migrations.");
    throw;
}

// HTTP request pipeline config
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// exposing this class to tests
namespace AuthenticationService.Api
{
    public partial class Program { }
}