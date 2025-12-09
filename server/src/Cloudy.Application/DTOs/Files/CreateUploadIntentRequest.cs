namespace Cloudy.Application.DTOs.Files;

public record CreateUploadIntentRequest(
    string FileName,
    string ContentType,
    long SizeBytes,
    int UserId,
    TimeSpan Ttl
);

