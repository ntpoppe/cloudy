namespace Cloudy.API.Requests.Files;

public record FinalizeRequest(
    string ObjectKey,
    string OriginalName,
    string ContentType,
    long SizeBytes
);

