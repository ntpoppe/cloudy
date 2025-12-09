using Cloudy.Application.DTOs;
using Cloudy.Application.DTOs.Files;
using Cloudy.Application.Interfaces.Services;
using Cloudy.Application.Interfaces.Repositories;
using Cloudy.Application.Mappers;
using Cloudy.Domain.ValueObjects;

namespace Cloudy.Application.Services;

public class FileService : IFileService
{
    private readonly IBlobStore _blobStore;
    private readonly IFileRepository _fileRepo;
    private readonly IUnitOfWork _uow;
    private readonly string _bucket;
    private readonly long _maxStorageBytes;

    public FileService(
        IFileRepository fileRepo,
        IUnitOfWork uow,
        IBlobStore blobStore,
        string bucket,
        long maxStorageBytes)
    {
        _fileRepo = fileRepo;
        _uow = uow;
        _blobStore = blobStore;
        _bucket = bucket;
        _maxStorageBytes = maxStorageBytes;
    }

    /// <summary>
    /// Create a presigned PUT URL for client upload.
    /// </summary>
    public async Task<CreateUploadIntentResponse> CreateUploadIntentAsync(CreateUploadIntentRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new ArgumentException("FileName is required.", nameof(request));

        // Check storage quota
        var canUpload = await CanUserUploadFileAsync(request.UserId, request.SizeBytes, ct);
        if (!canUpload)
        {
            var currentUsage = await GetUserStorageUsageAsync(request.UserId, ct);
            var maxBytes = GetUserStorageLimit(request.UserId);
            var availableSpace = maxBytes - currentUsage;
            throw new InvalidOperationException(
                $"Storage quota exceeded. Available space: {availableSpace / (1024 * 1024)}MB, " +
                $"Requested: {request.SizeBytes / (1024 * 1024)}MB, " +
                $"Total quota: {maxBytes / (1024 * 1024)}MB");
        }

        var objectKey = $"{Guid.NewGuid()}-{request.FileName}";

        // Get presigned PUT URL
        var url = await _blobStore.GetPresignedPutUrlAsync(_bucket, objectKey, request.Ttl);
        return new CreateUploadIntentResponse(objectKey, url, (int)request.Ttl.TotalSeconds);
    }

    /// <summary>
    /// Persist metadata after upload to blob storage.
    /// </summary>
    public async Task<FileDto> CreateMetadataAsync(CreateMetadataRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.ObjectKey))
            throw new ArgumentException("ObjectKey is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.OriginalName))
            throw new ArgumentException("OriginalName is required.", nameof(request));

        // Double-check quota before saving metadata
        var canUpload = await CanUserUploadFileAsync(request.UserId, request.SizeBytes, ct);
        if (!canUpload)
        {
            throw new InvalidOperationException("Storage quota exceeded during metadata creation.");
        }

        var metadata = new FileMetadata(request.ContentType, DateTime.UtcNow);
        var file = new Domain.Entities.File(request.OriginalName, request.SizeBytes, metadata, request.UserId);
        file.SetStorage(_bucket, request.ObjectKey);

        await _fileRepo.AddAsync(file, ct);
        await _uow.SaveChangesAsync(ct);
        return FileMapper.MapDto(file);
    }

    /// <summary>
    /// Gets a file by ID.
    /// </summary>
    public async Task<FileDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var f = await _fileRepo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException("file not found");
        return FileMapper.MapDto(f);
    }

    /// <summary>
    /// Supplies a presigned GET URL for download.
    /// </summary>
    public async Task<string> GetDownloadUrlAsync(GetDownloadUrlRequest request, CancellationToken ct = default)
    {
        var f = await _fileRepo.GetByIdAsync(request.FileId, ct)
                ?? throw new InvalidOperationException("file not found");

        return await _blobStore.GetPresignedGetUrlAsync(f.Bucket, f.ObjectKey, request.Ttl);
    }

    /// <summary>
    /// Renames a file. Metadata only.
    /// </summary>
    public async Task RenameAsync(RenameFileRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewName))
            throw new ArgumentException("NewName is required.", nameof(request));

        var f = await _fileRepo.GetByIdAsync(request.FileId, ct)
                ?? throw new InvalidOperationException("file not found");

        f.Rename(request.NewName, request.UserId);
        _fileRepo.Update(f);
        await _uow.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Deletes a file from blob storage and metadata table.
    /// </summary>
    public async Task DeleteAsync(DeleteFileRequest request, CancellationToken ct = default)
    {
        var f = await _fileRepo.GetByIdAsync(request.FileId, ct)
                ?? throw new InvalidOperationException("file not found");

        // Remove from blob storage
        await _blobStore.DeleteAsync(f.Bucket, f.ObjectKey);

        // Soft delete in DB
        f.SoftDelete(request.UserId);
        _fileRepo.Update(f);
        await _uow.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Marks a file as pending deletion. Shows up in trash on frontend.
    /// </summary>
    public async Task MarkAsPendingDeletionAsync(int id, int userId, CancellationToken ct = default)
    {
        var f = await _fileRepo.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("file not found");
        
        f.MarkAsPendingDeletion(userId);
        _fileRepo.Update(f);
        await _uow.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Restore a file from pending deletion.
    /// </summary>
    public async Task RestoreFromPendingDeletionAsync(int id, int userId, CancellationToken ct = default)
    {
        var f = await _fileRepo.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("file not found");
        
        f.RestoreFromPendingDeletion(userId);
        _fileRepo.Update(f);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<FileDto>> GetAllAsync(int userId, CancellationToken ct = default)
    {
        var files = await _fileRepo.GetByUserIdAsync(userId, ct);
        return files.Select(FileMapper.MapDto);
    }

    public async Task<long> GetUserStorageUsageAsync(int userId, CancellationToken ct = default)
    {
        return await _fileRepo.GetTotalStorageUsedByUserAsync(userId, ct);
    }

    public async Task<bool> CanUserUploadFileAsync(int userId, long fileSizeBytes, CancellationToken ct = default)
    {
        var currentUsage = await GetUserStorageUsageAsync(userId, ct);
        var maxBytes = GetUserStorageLimit(userId);
        return (currentUsage + fileSizeBytes) <= maxBytes;
    }

    public async Task<StorageUsageDto> GetStorageUsageAsync(int userId, CancellationToken ct = default)
    {
        var usedBytes = await GetUserStorageUsageAsync(userId, ct);
        var maxBytes = GetUserStorageLimit(userId);
        return new StorageUsageDto
        {
            UsedBytes = usedBytes,
            MaxBytes = maxBytes,
            UsagePercentage = (double)usedBytes / maxBytes * 100
        };
    }

    private long GetUserStorageLimit(int userId)
        => userId == 1 ? 250L * 1024 * 1024 * 1024 : _maxStorageBytes;
}

