using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cloudy.Infrastructure.Services;
using FluentAssertions;
using Minio;
using Minio.DataModel.Args;
using Moq;
using Xunit;

namespace Cloudy.Infrastructure.Tests.Services;

public class MinioBlobStoreTests
{
    private readonly Mock<IMinioClient> _minioMock = new();

    private MinioBlobStore CreateSut() => new MinioBlobStore(_minioMock.Object);

    [Fact]
    public async Task DeleteAsync_Should_Call_RemoveObjectAsync()
    {
        // Arrange
        _minioMock
            .Setup(m => m.RemoveObjectAsync(It.IsAny<RemoveObjectArgs>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var bucket = "cloudy";
        var objectKey = "to/delete.txt";

        // Act
        await sut.DeleteAsync(bucket, objectKey);

        // Assert
        _minioMock.Verify(m => m.RemoveObjectAsync(It.IsAny<RemoveObjectArgs>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPresignedGetUrlAsync_Should_Return_Url_From_Client()
    {
        // Arrange
        var expected = "http://signed-get-url";
        _minioMock
            .Setup(m => m.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
            .ReturnsAsync(expected);

        var sut = CreateSut();

        // Act
        var url = await sut.GetPresignedGetUrlAsync("cloudy", "obj.txt", expiry: System.TimeSpan.FromMinutes(5));

        // Assert
        url.Should().Be(expected);
        _minioMock.Verify(m => m.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()), Times.Once);
    }

    [Fact]
    public async Task GetPresignedPutUrlAsync_Should_Return_Url_From_Client()
    {
        // Arrange
        var expected = "http://signed-put-url";
        _minioMock
            .Setup(m => m.PresignedPutObjectAsync(It.IsAny<PresignedPutObjectArgs>()))
            .ReturnsAsync(expected);

        var sut = CreateSut();

        // Act
        var url = await sut.GetPresignedPutUrlAsync("cloudy", "obj.txt", expiry: System.TimeSpan.FromMinutes(5));

        // Assert
        url.Should().Be(expected);
        _minioMock.Verify(m => m.PresignedPutObjectAsync(It.IsAny<PresignedPutObjectArgs>()), Times.Once);
    }
}
