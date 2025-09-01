using Cloudy.Application.Interfaces;
using Cloudy.Domain.Entities;
using Cloudy.Infrastructure.Data;
using Cloudy.Infrastructure.Repositories;
using Cloudy.Infrastructure.Services;
using Cloudy.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace Cloudy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // DbContext
        services.AddDbContext<CloudyDbContext>(opts =>
            opts.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        // Repositories & UoW
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Services
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IFolderService, FolderService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        // MinIO
        var minioOpts = config.GetSection("Minio").Get<MinioSettings>()
           ?? throw new InvalidOperationException("Missing Minio config");
        var endpoint = minioOpts.Endpoint;
        var accessKey = minioOpts.AccessKey;
        var secretKey = minioOpts.SecretKey;
        services.AddMinio(cfg => cfg
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(false)
            .Build());

        services.AddScoped<IBlobStore, MinioBlobStore>();

        // Jwt
        services.Configure<JwtSettings>(config.GetSection("Jwt"));
        services.AddSingleton<IJwtService, JwtService>();

        return services;
    }
}
