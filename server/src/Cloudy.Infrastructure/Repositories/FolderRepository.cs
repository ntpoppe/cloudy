using Cloudy.Application.Interfaces;
using Cloudy.Domain.Entities;
using Cloudy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cloudy.Infrastructure.Repositories;

public class FolderRepository : IFolderRepository
{
    private readonly CloudyDbContext _context;

    public FolderRepository(CloudyDbContext context)
        => _context = context;

    public async Task<Folder?> GetByIdAsync(int id)
        => await _context.Folders.FirstOrDefaultAsync(f => f.Id == id);

    public async Task AddAsync(Folder folder)
        => await _context.Folders.AddAsync(folder);
    
    public void Update(Folder folder)
        => _context.Folders.Update(folder);

    public async Task<IEnumerable<Folder>> ListByParentAsync(int parentFolderId)
        => await _context.Folders.Where(f => f.ParentFolderId == parentFolderId).ToListAsync();
}