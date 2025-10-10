using Cloudy.Application.Interfaces.Repositories;
using Cloudy.Domain.Entities;
using Cloudy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cloudy.Infrastructure.Repositories;

public class FolderRepository : IFolderRepository
{
    private readonly CloudyDbContext _context;

    public FolderRepository(CloudyDbContext context)
        => _context = context;

    public async Task<Folder?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Folders.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task AddAsync(Folder folder, CancellationToken cancellationToken = default)
        => await _context.Folders.AddAsync(folder, cancellationToken);
    
    public void Update(Folder folder, CancellationToken cancellationToken = default)
        => _context.Folders.Update(folder);

    public async Task<IEnumerable<Folder>> ListByParentAsync(int parentFolderId, CancellationToken cancellationToken = default)
        => await _context.Folders.Where(f => f.ParentFolderId == parentFolderId).ToListAsync(cancellationToken);
}