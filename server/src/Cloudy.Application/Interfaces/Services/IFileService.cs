using Cloudy.Application.DTOs;

namespace Cloudy.Application.Interfaces;

public interface IFileService
{
    Task<(string ObjectKey, string Url, int ExpiresInSeconds)>
        CreateUploadIntentAsync(string fileName, string contentType, TimeSpan ttl, CancellationToken ct = default);

    Task<FileDto> CreateMetadataAsync(
        string objectKey,
        string originalName,
        string contentType,
        long sizeBytes,
        int userId,
        CancellationToken ct = default);

    Task<FileDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<string> GetDownloadUrlAsync(int id, TimeSpan ttl, CancellationToken ct = default);
    Task RenameAsync(int id, string newName, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<FileDto>> GetAllAsync(int userId, CancellationToken ct = default);
}