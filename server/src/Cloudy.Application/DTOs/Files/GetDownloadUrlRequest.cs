namespace Cloudy.Application.DTOs.Files;

public record GetDownloadUrlRequest(
    int FileId,
    TimeSpan Ttl
);

