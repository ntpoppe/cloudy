namespace Cloudy.Application.DTOs.Files;

public record DeleteFileRequest(
    int FileId,
    int UserId
);

