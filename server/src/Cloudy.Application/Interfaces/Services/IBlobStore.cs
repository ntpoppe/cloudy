namespace Cloudy.Application.Interfaces;

public interface IBlobStore
{
    Task UploadAsync(string bucket, string objectKey, Stream content, string contentType);
    Task<Stream> DownloadAsync(string bucket, string objectKey);
    Task DeleteAsync(string bucket, string objectKey);
    Task<string> GetPresignedGetUrlAsync(string bucket, string objectKey, TimeSpan expiry);
    Task<string> GetPresignedPutUrlAsync(string bucket, string objectKey, TimeSpan expiry);
}
