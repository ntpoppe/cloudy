using Cloudy.Application.DTOs;

namespace Cloudy.Application.Interfaces;

public interface IFileService
{
    Task<FileDto> UploadAsync(string name, Stream content, string contentType);
    Task<FileDto> GetByIdAsync(int id);
    Task RenameAsync(int id, string newName);
    Task DeleteAsync(int id);
}