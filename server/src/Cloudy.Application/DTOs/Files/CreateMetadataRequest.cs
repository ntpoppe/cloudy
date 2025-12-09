namespace Cloudy.Application.DTOs.Files;

public record CreateMetadataRequest(
    string ObjectKey,
    string OriginalName,
    string ContentType,
    long SizeBytes,
    int UserId
);

