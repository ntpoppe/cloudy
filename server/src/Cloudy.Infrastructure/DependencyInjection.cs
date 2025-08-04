using Cloudy.Application.Interfaces;
using Cloudy.Domain.Entities;
using Cloudy.Infrastructure.Data;
//using Cloudy.Infrastructure.FileStorage;
using Cloudy.Infrastructure.Repositories;
using Cloudy.Infrastructure.Services;
using Cloudy.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        // File storage
        // services.AddSingleton<IFileStorage, LocalFileStorage>();

        // Jwt
        services.Configure<JwtSettings>(config.GetSection("Jwt"));
        services.AddSingleton<IJwtService, JwtService>();

        return services;
    }
}
