using Cloudy.Application.Interfaces;
using Cloudy.Infrastructure.Services;
using Cloudy.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Options;
using Cloudy.Infrastructure.Settings;
using CloudyFile = Cloudy.Domain.Entities.File;

namespace Cloudy.Infrastructure.Tests.Services;

public class FileServiceTests
{
    private readonly Mock<IFileRepository> _fileRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IBlobStore> _blobStore = new();
    private readonly Mock<IOptions<MinioSettings>> _minioSettings = new();

    private FileService CreateSut()
    {
        _minioSettings.Setup(x => x.Value).Returns(new MinioSettings { Bucket = "test-bucket" });
        return new FileService(_fileRepo.Object, _uow.Object, _blobStore.Object, _minioSettings.Object);
    }

    [Fact]
    public async Task CreateUploadIntentAsync_Should_Return_PresignedUrl()
    {
        // Arrange
        var fileName = "test.txt";
        var contentType = "text/plain";
        var ttl = TimeSpan.FromMinutes(5);
        var expectedUrl = "https://presigned-put-url";
        
        _blobStore.Setup(b => b.GetPresignedPutUrlAsync(It.IsAny<string>(), It.IsAny<string>(), ttl))
            .ReturnsAsync(expectedUrl);

        var sut = CreateSut();

        // Act
        var result = await sut.CreateUploadIntentAsync(fileName, contentType, ttl);

        // Assert
        result.ObjectKey.Should().Contain(fileName);
        result.Url.Should().Be(expectedUrl);
        result.ExpiresInSeconds.Should().Be((int)ttl.TotalSeconds);
        _blobStore.Verify(b => b.GetPresignedPutUrlAsync("test-bucket", It.IsAny<string>(), ttl), Times.Once);
    }

    [Fact]
    public async Task CreateUploadIntentAsync_Should_Throw_If_FileName_Empty()
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert
        await sut.Invoking(s => s.CreateUploadIntentAsync("", "text/plain", TimeSpan.FromMinutes(5)))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*fileName is required*");
    }

    [Fact]
    public async Task CreateMetadataAsync_Should_Create_File_And_Save()
    {
        // Arrange
        var storageKey = "test-key";
        var originalName = "test.txt";
        var contentType = "text/plain";
        var sizeBytes = 1024L;
        var userId = 1;

        _fileRepo.Setup(r => r.AddAsync(It.IsAny<CloudyFile>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        var result = await sut.CreateMetadataAsync(storageKey, originalName, contentType, sizeBytes, userId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(originalName);
        result.Size.Should().Be(sizeBytes);
        result.ContentType.Should().Be(contentType);
        
        _fileRepo.Verify(r => r.AddAsync(It.Is<CloudyFile>(f => 
            f.Name == originalName && 
            f.Size == sizeBytes &&
            f.Metadata.ContentType == contentType &&
            f.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateMetadataAsync_Should_Throw_If_StorageKey_Empty()
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert
        await sut.Invoking(s => s.CreateMetadataAsync("", "test.txt", "text/plain", 1024, 1))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*objectKey is required*");
    }

    [Fact]
    public async Task CreateMetadataAsync_Should_Throw_If_OriginalName_Empty()
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert
        await sut.Invoking(s => s.CreateMetadataAsync("key", "", "text/plain", 1024, 1))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*originalName is required*");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_FileDto()
    {
        // Arrange
        var file = new CloudyFile("test.txt", 1024, new FileMetadata("text/plain", DateTime.UtcNow), 1);
        typeof(CloudyFile).GetProperty("Id")!.SetValue(file, 1);
        
        _fileRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(file);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("test.txt");
        result.Size.Should().Be(1024);
        result.ContentType.Should().Be("text/plain");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Throw_If_File_Not_Found()
    {
        // Arrange
        _fileRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CloudyFile?)null);

        var sut = CreateSut();

        // Act & Assert
        await sut.Invoking(s => s.GetByIdAsync(1))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*file not found*");
    }

    [Fact]
    public async Task GetDownloadUrlAsync_Should_Return_PresignedUrl()
    {
        // Arrange
        var file = new CloudyFile("test.txt", 1024, new FileMetadata("text/plain", DateTime.UtcNow), 1);
        typeof(CloudyFile).GetProperty("Id")!.SetValue(file, 1);
        file.SetStorage("test-bucket", "test-key");
        
        var expectedUrl = "https://presigned-get-url";
        var ttl = TimeSpan.FromMinutes(10);

        _fileRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(file);
        _blobStore.Setup(b => b.GetPresignedGetUrlAsync("test-bucket", "test-key", ttl))
            .ReturnsAsync(expectedUrl);

        var sut = CreateSut();

        // Act
        var result = await sut.GetDownloadUrlAsync(1, ttl);

        // Assert
        result.Should().Be(expectedUrl);
        _blobStore.Verify(b => b.GetPresignedGetUrlAsync("test-bucket", "test-key", ttl), Times.Once);
    }

    [Fact]
    public async Task GetDownloadUrlAsync_Should_Throw_If_File_Not_Found()
    {
        // Arrange
        _fileRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CloudyFile?)null);

        var sut = CreateSut();

        // Act & Assert
        await sut.Invoking(s => s.GetDownloadUrlAsync(1, TimeSpan.FromMinutes(5)))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*file not found*");
    }

    [Fact]
    public async Task RenameAsync_Should_Update_And_Save()
    {
        // Arrange
        var file = new CloudyFile("old.txt", 1024, new FileMetadata("text/plain", DateTime.UtcNow), 1);
        typeof(CloudyFile).GetProperty("Id")!.SetValue(file, 1);
        
        _fileRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(file);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        await sut.RenameAsync(1, "new.txt");

        // Assert
        file.Name.Should().Be("new.txt");
        _fileRepo.Verify(r => r.Update(file), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RenameAsync_Should_Throw_If_NewName_Empty()
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert
        await sut.Invoking(s => s.RenameAsync(1, ""))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*newName is required*");
    }

    [Fact]
    public async Task RenameAsync_Should_Throw_If_File_Not_Found()
    {
        // Arrange
        _fileRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CloudyFile?)null);

        var sut = CreateSut();

        // Act & Assert
        await sut.Invoking(s => s.RenameAsync(1, "new.txt"))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*file not found*");
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_From_BlobStore_And_Soft_Delete()
    {
        // Arrange
        var file = new CloudyFile("test.txt", 1024, new FileMetadata("text/plain", DateTime.UtcNow), 1);
        typeof(CloudyFile).GetProperty("Id")!.SetValue(file, 1);
        file.SetStorage("test-bucket", "test-key");
        
        _fileRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(file);
        _blobStore.Setup(b => b.DeleteAsync("test-bucket", "test-key"))
            .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        await sut.DeleteAsync(1);

        // Assert
        file.IsDeleted.Should().BeTrue();
        _blobStore.Verify(b => b.DeleteAsync("test-bucket", "test-key"), Times.Once);
        _fileRepo.Verify(r => r.Update(file), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Throw_If_File_Not_Found()
    {
        // Arrange
        _fileRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CloudyFile?)null);

        var sut = CreateSut();

        // Act & Assert
        await sut.Invoking(s => s.DeleteAsync(1))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*file not found*");
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_User_Files()
    {
        // Arrange
        var userId = 1;
        var files = new List<CloudyFile>
        {
            new("file1.txt", 1024, new FileMetadata("text/plain", DateTime.UtcNow), userId),
            new("file2.pdf", 2048, new FileMetadata("application/pdf", DateTime.UtcNow), userId)
        };
        
        typeof(CloudyFile).GetProperty("Id")!.SetValue(files[0], 1);
        typeof(CloudyFile).GetProperty("Id")!.SetValue(files[1], 2);
        files[0].SetStorage("test-bucket", "key1");
        files[1].SetStorage("test-bucket", "key2");

        _fileRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(files);

        var sut = CreateSut();

        // Act
        var result = await sut.GetAllAsync(userId);

        // Assert
        result.Should().NotBeNull();
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList[0].Id.Should().Be(1);
        resultList[0].Name.Should().Be("file1.txt");
        resultList[1].Id.Should().Be(2);
        resultList[1].Name.Should().Be("file2.pdf");

        _fileRepo.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Empty_When_No_Files()
    {
        // Arrange
        var userId = 1;
        _fileRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CloudyFile>());

        var sut = CreateSut();

        // Act
        var result = await sut.GetAllAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _fileRepo.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

