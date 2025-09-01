using Cloudy.Application.Interfaces;
using Cloudy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cloudy.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly CloudyDbContext _context;

    public FileRepository(CloudyDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.File?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Files.FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public async Task<IEnumerable<Domain.Entities.File>> GetAllAsync(CancellationToken ct = default)
    {
        return await Task.FromResult(_context.Files.AsEnumerable());
    }

    public async Task<IEnumerable<Domain.Entities.File>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Files
            .Where(f => f.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Domain.Entities.File file, CancellationToken ct = default)
    {
        await _context.Files.AddAsync(file, ct);
    }

    public void Update(Domain.Entities.File file)
    {
        _context.Files.Update(file);
    }
}