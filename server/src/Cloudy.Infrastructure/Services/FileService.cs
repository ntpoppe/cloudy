using Cloudy.Application.DTOs;
using Cloudy.Application.Interfaces;
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

    public FileService(
        IFileRepository fileRepo,
        IUnitOfWork uow,
        IBlobStore blobStore,
        IOptions<MinioSettings> minioSettings)
    {
        _fileRepo = fileRepo;
        _uow = uow;
        _blobStore = blobStore;
        _bucket = minioSettings.Value.Bucket;
    }

    // Create presigned PUT for client upload
    public async Task<(string ObjectKey, string Url, int ExpiresInSeconds)>
        CreateUploadIntentAsync(string fileName, string contentType, TimeSpan ttl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("fileName is required.", nameof(fileName));

        var objectKey = $"{Guid.NewGuid()}-{fileName}";

        // MinIO presigned PUT
        var url = await _blobStore.GetPresignedPutUrlAsync(_bucket, objectKey, ttl);
        return (objectKey, url, (int)ttl.TotalSeconds);
    }

    // Persist metadata after upload completed to MinIO
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

        var metadata = new FileMetadata(contentType, DateTime.UtcNow);
        var file = new Domain.Entities.File(originalName, sizeBytes, metadata, userId);
        file.SetStorage(_bucket, objectKey);

        await _fileRepo.AddAsync(file, ct);
        await _uow.SaveChangesAsync(ct);
        return FileMapper.MapDto(file);
    }

    public async Task<FileDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var f = await _fileRepo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException("file not found");
        return FileMapper.MapDto(f);
    }

    // Presigned GET for download
    public async Task<string> GetDownloadUrlAsync(int id, TimeSpan ttl, CancellationToken ct = default)
    {
        var f = await _fileRepo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException("file not found");

        // MinIO presigned GET
        return await _blobStore.GetPresignedGetUrlAsync(f.Bucket, f.ObjectKey, ttl);
    }

    //  Rename (metadata only)
    public async Task RenameAsync(int id, string newName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("newName is required.", nameof(newName));

        var f = await _fileRepo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException("file not found");

        f.Rename(newName);
        _fileRepo.Update(f);
        await _uow.SaveChangesAsync(ct);
    }

    // Delete (MinIO + soft delete)
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var f = await _fileRepo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException("file not found");

        // Remove from MinIO
        await _blobStore.DeleteAsync(f.Bucket, f.ObjectKey);

        // Soft delete in DB
        f.SoftDelete();
        _fileRepo.Update(f);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<FileDto>> GetAllAsync(int userId, CancellationToken ct = default)
    {
        var files = await _fileRepo.GetByUserIdAsync(userId, ct);
        return files.Select(FileMapper.MapDto);
    }
}
