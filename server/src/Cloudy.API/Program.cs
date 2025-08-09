using System.Text;
using Cloudy.Application;
using Cloudy.Infrastructure;
using Cloudy.Infrastructure.Data;
using Cloudy.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//----------------------
// Configuration
//----------------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
if (jwt == null || string.IsNullOrWhiteSpace(jwt.Key))
    throw new InvalidOperationException("Missing or invalid JWT configuration.");

//----------------------
// Service Registration
//----------------------
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();