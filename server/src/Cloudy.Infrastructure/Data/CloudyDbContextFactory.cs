using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Cloudy.Infrastructure.Data;
public class CloudyDbContextFactory : IDesignTimeDbContextFactory<CloudyDbContext>
{
    public CloudyDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var conn = config["DB_CONNSTRING"]
                   ?? Environment.GetEnvironmentVariable("DB_CONNSTRING")
                   ?? throw new InvalidOperationException(
                       "No connection string found. Make sure the .env file was loaded correctly."
                   );

        var builder = new DbContextOptionsBuilder<CloudyDbContext>();
        builder.UseNpgsql(conn);

        return new CloudyDbContext(builder.Options);
    }
}