using Microsoft.IdentityModel.Tokens;
using System.Text;
using RealEstateApi.Infrastructure.Services;
using RealEstateApi.Infrastructure.Repositories;
using RealEstateApi.Infrastructure.Data;
using RealEstateApi.Infrastructure.Settings;
using RealEstateApi.Application.Interfaces;
using RealEstateApi.Application.Services;
using RealEstateApi.Domain.Entities;
using DotNetEnv;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.Reflection;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure JWT
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT_KEY is not set in the environment variables.");
}

var jwtSettings = new JwtSettings
{
    Key = jwtKey,
    Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "default-issuer",
    Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "default-audience",
    ExpiresMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRES_MINUTES"), out var expires) ? expires : 60
};

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<IJwtService, JwtService>();

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoDbContext, MongoDbContext>();

builder.Services.AddScoped<IOwnerRepository, OwnerRepository>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPropertyImageRepository, PropertyImageRepository>();
builder.Services.AddScoped<IPropertyService, PropertyService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// Configure JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
    });
}

app.MapControllers();

app.UseHttpsRedirection();

app.Run();
