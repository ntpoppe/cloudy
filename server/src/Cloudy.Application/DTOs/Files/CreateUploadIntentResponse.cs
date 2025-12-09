namespace Cloudy.Application.DTOs.Files;

public record CreateUploadIntentResponse(
    string ObjectKey,
    string Url,
    int ExpiresInSeconds
);

