using Cloudy.Domain.Entities;
using Cloudy.Application.DTOs;

namespace Cloudy.Application.Mappers;

public static class FolderMapper
{
    public static FolderDto MapDto(Folder folder)
        => new FolderDto(
            folder.Id,
            folder.Name,
            folder.ParentFolderId,
            folder.CreatedAt
        );
}