using Cloudy.Application.Interfaces.Repositories;
using Cloudy.Application.Interfaces.Services;
using Cloudy.Application.Services;
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

        // Password Hasher (Infrastructure adapter)
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IPasswordHasher, PasswordHasherAdapter>();

        // MinIO
        var minioOpts = config.GetSection("Minio").Get<MinioSettings>()
           ?? throw new InvalidOperationException("Missing Minio config");
        var endpoint = minioOpts.Endpoint;
        var accessKey = minioOpts.AccessKey;
        var secretKey = minioOpts.SecretKey;
        var useSSL = minioOpts.UseSSL;
        services.AddMinio(cfg => cfg
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(useSSL)
            .Build());

        services.AddScoped<IBlobStore, MinioBlobStore>();

        // Application Services (wired with Infrastructure settings)
        var storageOpts = config.GetSection("Storage").Get<StorageSettings>()
           ?? throw new InvalidOperationException("Missing Storage config");
        
        services.AddScoped<IFileService>(sp =>
        {
            var fileRepo = sp.GetRequiredService<IFileRepository>();
            var uow = sp.GetRequiredService<IUnitOfWork>();
            var blobStore = sp.GetRequiredService<IBlobStore>();
            
            return new Application.Services.FileService(
                fileRepo,
                uow,
                blobStore,
                minioOpts.Bucket,
                storageOpts.MaxStorageBytes
            );
        });

        services.AddScoped<IFolderService>(sp =>
        {
            var folderRepo = sp.GetRequiredService<IFolderRepository>();
            var uow = sp.GetRequiredService<IUnitOfWork>();
            
            return new Application.Services.FolderService(folderRepo, uow);
        });

        services.AddScoped<IUserService>(sp =>
        {
            var userRepo = sp.GetRequiredService<IUserRepository>();
            var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
            
            return new Application.Services.UserService(userRepo, passwordHasher);
        });

        // JWT Service
        var jwtOpts = config.GetSection("Jwt").Get<JwtSettings>()
           ?? throw new InvalidOperationException("Missing Jwt config");
        
        services.AddSingleton<IJwtService>(_ =>
            new Application.Services.JwtService(
                jwtOpts.Key,
                jwtOpts.Issuer,
                jwtOpts.Audience,
                jwtOpts.ExpiryMinutes
            ));

        return services;
    }
}
