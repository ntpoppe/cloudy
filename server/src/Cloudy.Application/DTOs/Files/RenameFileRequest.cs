namespace Cloudy.Application.DTOs.Files;

public record RenameFileRequest(
    int FileId,
    int UserId,
    string NewName
);

