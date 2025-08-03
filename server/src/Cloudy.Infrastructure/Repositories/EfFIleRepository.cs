using Cloudy.Application.Interfaces;
using Cloudy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cloudy.Infrastructure.Repositories;

public class EfFileRepository : IFileRepository
{
    private readonly CloudyDbContext _context;
    
    public EfFileRepository(CloudyDbContext context)
        => _context = context;
    public Task<Domain.Entities.File?> GetByIdAsync(int id)
        => _context.Files.FirstOrDefaultAsync(f => f.Id == id);

    public async Task AddAsync(Domain.Entities.File file)
        => await _context.Files.AddAsync(file);

    public void Update(Domain.Entities.File file)
        => _context.Files.Update(file);
}