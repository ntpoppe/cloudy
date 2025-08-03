using Cloudy.Application.Interfaces;
using Cloudy.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cloudy.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IFolderService, FolderService>();
        return services;
    }
}