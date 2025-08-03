using Cloudy.Application.Mappers;
using Cloudy.Application.DTOs;
using Cloudy.Application.Interfaces;
using Cloudy.Domain.ValueObjects;

namespace Cloudy.Application.Services;

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepo;
    private readonly IUnitOfWork _uow;

    public FileService(IFileRepository fileRepo, IUnitOfWork uow)
    {
        _fileRepo = fileRepo;
        _uow = uow;
    }

    public async Task<FileDto> UploadAsync(string name, Stream content, string contentType)
    {
        long size;
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms);
        size = ms.Length;

        var metadata = new FileMetadata(contentType, DateTime.UtcNow);
        var file = new Domain.Entities.File(name, size, metadata);
        await _fileRepo.AddAsync(file);
        await _uow.SaveChangesAsync();

        return FileMapper.MapDto(file);
    }

    public async Task<FileDto> GetByIdAsync(int id)
    {
        var file = await _fileRepo.GetByIdAsync(id);
        if (file == null)
            throw new InvalidOperationException("'file' not found in FileService.GetByIdAsync");

        return FileMapper.MapDto(file);
    }

    public async Task RenameAsync(int id, string newName)
    {
        var file = await _fileRepo.GetByIdAsync(id);
        if (file == null)
            throw new InvalidOperationException("'file' null in FileService.RenameAsync");
        file.Rename(newName);
        _fileRepo.Update(file);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var file = await _fileRepo.GetByIdAsync(id);
        if (file == null)
            throw new InvalidOperationException("'file' null in FileService.DeleteAsync");

        file.SoftDelete();
        _fileRepo.Update(file);
        await _uow.SaveChangesAsync();
    }
}