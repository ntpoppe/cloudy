using Cloudy.Domain.Entities;

namespace Cloudy.Application.Interfaces;

public interface IFolderRepository
{
    Task<Folder?> GetByIdAsync(int id);
    Task AddAsync(Folder folder);
    void Update(Folder folder);
    Task<IEnumerable<Folder>> ListByParentAsync(int parentFolderId);
}