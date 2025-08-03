namespace Cloudy.Application.DTOs;

public record FileDto(
    int Id,
    string Name,
    long Size,
    string ContentType,
    DateTime UploadedAt
);