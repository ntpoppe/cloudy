using Cloudy.Application.DTOs;

namespace Cloudy.Application.Interfaces.Services;

public interface IFileService
{
    Task<(string ObjectKey, string Url, int ExpiresInSeconds)>
        CreateUploadIntentAsync(string fileName, string contentType, long sizeBytes, int userId, TimeSpan ttl, CancellationToken ct = default);

    Task<FileDto> CreateMetadataAsync(
        string objectKey,
        string originalName,
        string contentType,
        long sizeBytes,
        int userId,
        CancellationToken ct = default);

    Task<FileDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<string> GetDownloadUrlAsync(int id, TimeSpan ttl, CancellationToken ct = default);
    Task RenameAsync(int id, int userId, string newName, CancellationToken ct = default);
    Task DeleteAsync(int id, int userId, CancellationToken ct = default);
    Task<IEnumerable<FileDto>> GetAllAsync(int userId, CancellationToken ct = default);
    Task<long> GetUserStorageUsageAsync(int userId, CancellationToken ct = default);
    Task<bool> CanUserUploadFileAsync(int userId, long fileSizeBytes, CancellationToken ct = default);
    Task<StorageUsageDto> GetStorageUsageAsync(int userId, CancellationToken ct = default);
}