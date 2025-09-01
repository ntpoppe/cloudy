namespace Cloudy.Application.Interfaces;

public interface IBlobStore
{
    // TODO: Cancellation tokens..?
    Task<string> GetPresignedGetUrlAsync(string bucket, string objectKey, TimeSpan expiry);
    Task<string> GetPresignedPutUrlAsync(string bucket, string objectKey, TimeSpan expiry);
    Task DeleteAsync(string bucket, string objectKey);
}
