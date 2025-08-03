using Cloudy.Application.Interfaces;
using Cloudy.Infrastructure.Data;
//using Cloudy.Infrastructure.FileStorage;
using Cloudy.Infrastructure.Repositories;
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
        services.AddScoped<IFileRepository, EfFileRepository>();
        services.AddScoped<IFolderRepository, EfFolderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // File storage
        // services.AddSingleton<IFileStorage, LocalFileStorage>();

        return services;
    }
}
