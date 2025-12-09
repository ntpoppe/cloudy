using Cloudy.Application.DTOs;
using Cloudy.Application.DTOs.Files;

namespace Cloudy.Application.Interfaces.Services;

public interface IFileService
{
    Task<CreateUploadIntentResponse> CreateUploadIntentAsync(CreateUploadIntentRequest request, CancellationToken ct = default);
    Task<FileDto> CreateMetadataAsync(CreateMetadataRequest request, CancellationToken ct = default);
    Task<FileDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<string> GetDownloadUrlAsync(GetDownloadUrlRequest request, CancellationToken ct = default);
    Task RenameAsync(RenameFileRequest request, CancellationToken ct = default);
    Task DeleteAsync(DeleteFileRequest request, CancellationToken ct = default);
    Task MarkAsPendingDeletionAsync(int id, int userId, CancellationToken ct = default);
    Task RestoreFromPendingDeletionAsync(int id, int userId, CancellationToken ct = default);
    Task<IEnumerable<FileDto>> GetAllAsync(int userId, CancellationToken ct = default);
    Task<long> GetUserStorageUsageAsync(int userId, CancellationToken ct = default);
    Task<bool> CanUserUploadFileAsync(int userId, long fileSizeBytes, CancellationToken ct = default);
    Task<StorageUsageDto> GetStorageUsageAsync(int userId, CancellationToken ct = default);
}