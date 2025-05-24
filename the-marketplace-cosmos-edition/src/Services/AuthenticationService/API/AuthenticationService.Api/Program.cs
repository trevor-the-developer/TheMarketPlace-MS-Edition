using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AuthenticationService.Application.Extensions;
using AuthenticationService.Application.Settings;
using AuthenticationService.Persistence.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure settings
var authServiceConfiguration = builder.Configuration
    .GetSection("AuthenticationService")
    .Get<AuthenticationServiceConfiguration>();

builder.Services.AddSingleton(authServiceConfiguration);

// Application services
builder.Services.AddApplicationServices();

// Persistence services
builder.Services.AddPersistenceServices(authServiceConfiguration);

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