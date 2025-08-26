using Minio;
using Minio.DataModel.Args;
using Cloudy.Application.Interfaces;

namespace Cloudy.Infrastructure.Services;

public class MinioBlobStore : IBlobStore
{
    private readonly IMinioClient _client;

    public MinioBlobStore(IMinioClient client) => _client = client;

    public async Task UploadAsync(string bucket, string objectKey, Stream content, string contentType)
    {
        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(content.Length)
            .WithContentType(contentType));
    }

    public async Task<Stream> DownloadAsync(string bucket, string objectKey)
    {
        var ms = new MemoryStream();
        await _client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithCallbackStream(s => s.CopyTo(ms)));
        ms.Position = 0;
        return ms;
    }

    public async Task DeleteAsync(string bucket, string objectKey) =>
        await _client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey));

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
}
