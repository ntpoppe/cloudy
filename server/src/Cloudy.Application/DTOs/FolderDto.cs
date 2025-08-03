namespace Cloudy.Application.DTOs;

public record FolderDto(
    int Id,
    string Name,
    int? ParentFolderId,
    DateTime CreatedAt
);