using Cloudy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cloudy.Infrastructure.Tests.TestHelpers;

public static class InMemoryContextFactory
{
    public static CloudyDbContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<CloudyDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        var context = new CloudyDbContext(options);
        return context;
    }
}