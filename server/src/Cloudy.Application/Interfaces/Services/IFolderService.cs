using Cloudy.Application.DTOs;

namespace Cloudy.Application.Interfaces;

public interface IFolderService
{
    Task<FolderDto> CreateAsync(string name, int? parentFolderId = null, CancellationToken cancellationToken = default);
    Task<FolderDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task RenameAsync(int id, int userId, string newName, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FolderDto>> ListAsync(int parentFolderId, CancellationToken cancellationToken = default);
}