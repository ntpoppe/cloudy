using Minio;
using Minio.DataModel.Args;
using Cloudy.Application.Interfaces.Services;

namespace Cloudy.Infrastructure.Services;

public class MinioBlobStore : IBlobStore
{
    private readonly IMinioClient _client;

    public MinioBlobStore(IMinioClient client) => _client = client;

    public Task<string> GetPresignedGetUrlAsync(string bucket, string objectKey, TimeSpan expiry) =>
        _client.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithExpiry((int)expiry.TotalSeconds));

    public Task<string> GetPresignedPutUrlAsync(string bucket, string objectKey, TimeSpan expiry) =>
        _client.PresignedPutObjectAsync(new PresignedPutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithExpiry((int)expiry.TotalSeconds));

    public async Task DeleteAsync(string bucket, string objectKey) =>
        await _client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey));
}
