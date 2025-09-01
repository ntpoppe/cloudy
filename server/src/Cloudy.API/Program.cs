using System;
using System.Text;
using Cloudy.Infrastructure;
using Cloudy.Infrastructure.Data;
using Cloudy.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DotNetEnv;

//----------------------
// Environment as argument (local debugging without docker)
//----------------------
var envFile = GetArgValue(args, "--env-file");
if (!string.IsNullOrWhiteSpace(envFile) && File.Exists(envFile))
    Env.Load(envFile);

var builder = WebApplication.CreateBuilder(args);

//----------------------
// Configuration
//----------------------
SetDefaultConnectionFromEnvIfExists(builder, envFile);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
if (jwt == null || string.IsNullOrWhiteSpace(jwt.Key))
    throw new InvalidOperationException("Missing or invalid JWT configuration.");

// MinIO configuration check
var minioSection = builder.Configuration.GetSection("MINIO");
var minioEndpoint = minioSection["Endpoint"];
var minioAccessKey = minioSection["AccessKey"];
var minioSecretKey = minioSection["SecretKey"];
if (string.IsNullOrWhiteSpace(minioEndpoint))
    throw new InvalidOperationException("MinIO endpoint missing (Minio:Endpoint). Check your --env-file and keys.");
if (string.IsNullOrWhiteSpace(minioAccessKey) || string.IsNullOrWhiteSpace(minioSecretKey))
    throw new InvalidOperationException("MinIO credentials missing (Minio:AccessKey/SecretKey).");

//----------------------
// Service Registration
//----------------------
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

//----------------------
// CORS
//----------------------
const string CorsPolicy = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:3000",
                "http://localhost:4200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


//----------------------
// Authentication & Authorization
//----------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var keyBytes = Encoding.UTF8.GetBytes(jwt.Key);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt.Issuer,
        ValidateAudience = true,
        ValidAudience = jwt.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

//----------------------
// Swagger / OpenAPI
//----------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cloudy API", Version = "v1" });
});

//----------------------
// Build App
//----------------------
var app = builder.Build();

//----------------------
// Database Migration
//----------------------
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CloudyDbContext>();
    await db.Database.MigrateAsync();
}

//----------------------
// Middleware Pipeline
//----------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

// Only redirect in production because the container listens on HTTP
// if (app.Environment.IsProduction())
// {
//     app.UseHttpsRedirection();
// }

app.UseRouting();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static string? GetArgValue(string[] argv, string name)
{
    for (int i = 0; i < argv.Length - 1; i++)
        if (argv[i] == name) return argv[i + 1];
    return null;
}

/// <summary>
/// Set the default connection string from the environment variable DB_CONNSTRING
/// This is used when docker isn't used. `dotnet run --env-file .env`
/// </summary>
static void SetDefaultConnectionFromEnvIfExists(WebApplicationBuilder builder, string? envFile)
{
    if (envFile is null) return;

    builder.Configuration.AddEnvironmentVariables();
    var existing = builder.Configuration.GetConnectionString("DefaultConnection");
    var dbConn = builder.Configuration["DB_CONNSTRING"]
             ?? Environment.GetEnvironmentVariable("DB_CONNSTRING");

    if (string.IsNullOrWhiteSpace(existing) && !string.IsNullOrWhiteSpace(dbConn))
    {
        builder.Configuration["ConnectionStrings:DefaultConnection"] = dbConn;
        Console.WriteLine("DefaultConnection set from DB_CONNSTRING");
    }
}