using Cloudy.Domain.Entities;
using Cloudy.Application.DTOs;

namespace Cloudy.Application.Mappers;

public static class FolderMapper
{
    public static FolderDto MapDto(Folder folder)
        => new FolderDto() 
        {
            Id = folder.Id,
            Name = folder.Name,
            ParentFolderId = folder.ParentFolderId,
            CreatedAt = folder.CreatedAt
        };
}