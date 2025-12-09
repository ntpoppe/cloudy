using Cloudy.Application.Mappers;
using Cloudy.Application.DTOs;
using Cloudy.Application.Interfaces.Repositories;
using Cloudy.Application.Interfaces.Services;

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

    public async Task<FolderDto> CreateAsync(string name, int? parentFolderId = null, CancellationToken cancellationToken = default)
    {
        var folder = new Domain.Entities.Folder(name, parentFolderId);
        await _folderRepo.AddAsync(folder, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return FolderMapper.MapDto(folder);
    }

    public async Task<FolderDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var folder = await _folderRepo.GetByIdAsync(id, cancellationToken);
        if (folder == null)
            throw new InvalidOperationException("'folder' null in FolderService.GetByIdAsync");

        return FolderMapper.MapDto(folder);
    }

    public async Task RenameAsync(int id, int userId, string name, CancellationToken cancellationToken = default)
    {
        var folder = await _folderRepo.GetByIdAsync(id, cancellationToken);
        if (folder == null)
            throw new InvalidOperationException("'folder' null in FolderService.RenameAsync");
        
        folder.Rename(name);
        _folderRepo.Update(folder, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        var folder = await _folderRepo.GetByIdAsync(id, cancellationToken);
        if (folder == null)
            throw new InvalidOperationException("'folder' null in FolderService.DeleteAsync");

        folder.SoftDelete(userId);
        _folderRepo.Update(folder, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<FolderDto>> ListAsync(int parentFolderId, CancellationToken cancellationToken = default)
    {
        var folders = await _folderRepo.ListByParentAsync(parentFolderId, cancellationToken);
        if (folders == null)
            throw new InvalidOperationException("'folders' null in FolderService.ListAsync");

        return new List<FolderDto>(
            folders.Select(f => FolderMapper.MapDto(f)
        ));
    }
}

