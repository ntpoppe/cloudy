using Cloudy.Application.DTOs;

namespace Cloudy.Application.Interfaces;

public interface IFolderService
{
    Task<FolderDto> CreateAsync(string name, int? parentFolderId = null);
    Task<FolderDto> GetByIdAsync(int id);
    Task RenameAsync(int id, string newName);
    Task DeleteAsync(int id);
    Task<IEnumerable<FolderDto>> ListAsync(int parentFolderId);
}