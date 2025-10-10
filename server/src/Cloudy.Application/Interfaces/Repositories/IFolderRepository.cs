using Cloudy.Domain.Entities;

namespace Cloudy.Application.Interfaces.Repositories;

public interface IFolderRepository
{
    Task<Folder?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Folder folder, CancellationToken cancellationToken = default);
    void Update(Folder folder, CancellationToken cancellationToken = default);
    Task<IEnumerable<Folder>> ListByParentAsync(int parentFolderId, CancellationToken cancellationToken = default);
}