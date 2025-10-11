using Cloudy.Application.DTOs;
using Cloudy.Application.Interfaces.Services;
using Cloudy.Application.Interfaces.Repositories;
using Cloudy.Application.Mappers;
using Cloudy.Domain.ValueObjects;
using Cloudy.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Cloudy.Infrastructure.Services;

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
        IOptions<MinioSettings> minioSettings,
        IOptions<StorageSettings> storageSettings)
    {
        _fileRepo = fileRepo;
        _uow = uow;
        _blobStore = blobStore;
        _bucket = minioSettings.Value.Bucket;
        _maxStorageBytes = storageSettings.Value.MaxStorageBytes;
    }

    /// <summary>
    /// Create a presigned MinIO PUT for client upload.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="contentType"></param>
    /// <param name="sizeBytes"></param>
    /// <param name="userId"></param>
    /// <param name="ttl"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<(string ObjectKey, string Url, int ExpiresInSeconds)>
        CreateUploadIntentAsync(string fileName, string contentType, long sizeBytes, int userId, TimeSpan ttl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("fileName is required.", nameof(fileName));

        // Check storage quota
        var canUpload = await CanUserUploadFileAsync(userId, sizeBytes, ct);
        if (!canUpload)
        {
            var currentUsage = await GetUserStorageUsageAsync(userId, ct);
            var maxBytes = GetUserStorageLimit(userId);
            var availableSpace = maxBytes - currentUsage;
            throw new InvalidOperationException(
                $"Storage quota exceeded. Available space: {availableSpace / (1024 * 1024)}MB, " +
                $"Requested: {sizeBytes / (1024 * 1024)}MB, " +
                $"Total quota: {maxBytes / (1024 * 1024)}MB");
        }

        var objectKey = $"{Guid.NewGuid()}-{fileName}";

        // MinIO pre-signed PUT
        var url = await _blobStore.GetPresignedPutUrlAsync(_bucket, objectKey, ttl);
        return (objectKey, url, (int)ttl.TotalSeconds);
    }

    /// <summary>
    /// Persist metadata after upload to MinIO.
    /// </summary>
    /// <param name="objectKey"></param>
    /// <param name="originalName"></param>
    /// <param name="contentType"></param>
    /// <param name="sizeBytes"></param>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<FileDto> CreateMetadataAsync(
        string objectKey,
        string originalName,
        string contentType,
        long sizeBytes,
        int userId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            throw new ArgumentException("objectKey is required.", nameof(objectKey));
        if (string.IsNullOrWhiteSpace(originalName))
            throw new ArgumentException("originalName is required.", nameof(originalName));

        // Double-check quota before saving metadata
        var canUpload = await CanUserUploadFileAsync(userId, sizeBytes, ct);
        if (!canUpload)
        {
            throw new InvalidOperationException("Storage quota exceeded during metadata creation.");
        }

        var metadata = new FileMetadata(contentType, DateTime.UtcNow);
        var file = new Domain.Entities.File(originalName, sizeBytes, metadata, userId);
        file.SetStorage(_bucket, objectKey);

        await _fileRepo.AddAsync(file, ct);
        await _uow.SaveChangesAsync(ct);
        return FileMapper.MapDto(file);
    }

    /// <summary>
    /// Gets a file by ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<FileDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var f = await _fileRepo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException("file not found");
        return FileMapper.MapDto(f);
    }

    /// <summary>
    /// Supplies a presigned MinIO GET request for download.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ttl"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string> GetDownloadUrlAsync(int id, TimeSpan ttl, CancellationToken ct = default)
    {
        var f = await _fileRepo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException("file not found");

        // MinIO presigned GET
        return await _blobStore.GetPresignedGetUrlAsync(f.Bucket, f.ObjectKey, ttl);
    }

    /// <summary>
    /// Renames a file. Metadata only.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <param name="newName"></param>
    /// <param name="ct"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task RenameAsync(int id, int userId, string newName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("newName is required.", nameof(newName));

        var f = await _fileRepo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException("file not found");

        f.Rename(newName, userId);
        _fileRepo.Update(f);
        await _uow.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Deletes a file from MinIO storage and metadata table.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task DeleteAsync(int id, int userId, CancellationToken ct = default)
    {
        var f = await _fileRepo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException("file not found");

        // Remove from MinIO
        await _blobStore.DeleteAsync(f.Bucket, f.ObjectKey);

        // Soft delete in DB
        f.SoftDelete(userId);
        _fileRepo.Update(f);
        await _uow.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Marks a file as pending deletion. Shows up in trash on frontend.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    /// <exception cref="InvalidOperationException"></exception>
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
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    /// <exception cref="InvalidOperationException"></exception>
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
