namespace Cloudy.API.Requests.Files;

public record CreateIntentRequest(
    string FileName,
    string ContentType,
    long SizeBytes
);

