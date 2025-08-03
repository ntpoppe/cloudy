using System.Reflection;
using Cloudy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public class CloudyDbContextFactory : IDesignTimeDbContextFactory<CloudyDbContext>
{
    public CloudyDbContext CreateDbContext(string[] args)
    {
        // Find the path to the Cloudy.API project
        var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var projectRoot = Path.GetFullPath(Path.Combine(
            exePath,
            "..", "..", "..", "..",    // back out of bin/Debug/netX
            "Cloudy.API"               // into the API project
        ));

        // Build configuration, but make both files optional
        var config = new ConfigurationBuilder()
            .SetBasePath(projectRoot)
            .AddJsonFile("appsettings.json") // could make optional: true
            .AddJsonFile("appsettings.Development.json") // could make optional: true
            .AddEnvironmentVariables()
            .Build();

        // Try JSON first, then environment 
        var conn = config.GetConnectionString("DefaultConnection")
                    ?? Environment.GetEnvironmentVariable("DefaultConnection")
                    ?? throw new InvalidOperationException(
                        "No connection string found. Place it in appsettings or set the DefaultConnection env var."
                        );

        var builder = new DbContextOptionsBuilder<CloudyDbContext>();
        builder.UseNpgsql(conn);

        return new CloudyDbContext(builder.Options);
    }
}