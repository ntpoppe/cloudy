using Cloudy.Application.Interfaces;
using Cloudy.Infrastructure.Data;

namespace Cloudy.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CloudyDbContext _context;

    public UnitOfWork(CloudyDbContext context)
        => _context = context;

    public Task<int> SaveChangesAsync()
        => _context.SaveChangesAsync();
}
