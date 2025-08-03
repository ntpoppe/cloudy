using Cloudy.Application.Mappers;
using Cloudy.Application.DTOs;
using Cloudy.Application.Interfaces;
using Cloudy.Domain.Entities;

namespace Cloudy.Application.Services;

public class FolderService : IFolderService
{
    private readonly IFolderRepository _folderRepo;
    private readonly IUnitOfWork _uow;

    public FolderService(IFolderRepository folderRepo, IUnitOfWork uow)
    {
        _folderRepo = folderRepo;
        _uow = uow;
    }

    public async Task<FolderDto> CreateAsync(string name, int? parentFolderId = null)
    {
        var folder = new Folder(name, parentFolderId);
        await _folderRepo.AddAsync(folder);
        await _uow.SaveChangesAsync();
        return FolderMapper.MapDto(folder);
    }

    public async Task<FolderDto> GetByIdAsync(int id)
    {
        var folder = await _folderRepo.GetByIdAsync(id);
        if (folder == null)
            throw new InvalidOperationException("'folder' null in FolderService.GetByIdAsync");

        return FolderMapper.MapDto(folder);
    }

    public async Task RenameAsync(int id, string name)
    {
        var folder = await _folderRepo.GetByIdAsync(id);
        if (folder == null)
            throw new InvalidOperationException("'folder' null in FolderService.RenameAsync");
        
        folder.Rename(name);
        _folderRepo.Update(folder);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var folder = await _folderRepo.GetByIdAsync(id);
        if (folder == null)
            throw new InvalidOperationException("'folder' null in FolderService.DeleteAsync");

        folder.SoftDelete();
        _folderRepo.Update(folder);
        await _uow.SaveChangesAsync();
    }

    public async Task<IEnumerable<FolderDto>> ListAsync(int parentFolderId)
    {
        var folders = await _folderRepo.ListByParentAsync(parentFolderId);
        if (folders == null)
            throw new InvalidOperationException("'folders' null in FolderService.ListAsync");

        return new List<FolderDto>(
            folders.Select(f => FolderMapper.MapDto(f)
        ));
    }
}