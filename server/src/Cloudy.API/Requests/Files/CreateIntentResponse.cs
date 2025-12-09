namespace Cloudy.API.Requests.Files;

public record CreateIntentResponse(
    string UploadUrl,
    string FileId
);

