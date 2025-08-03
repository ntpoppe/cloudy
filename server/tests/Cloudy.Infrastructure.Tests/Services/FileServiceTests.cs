using Cloudy.Application.Interfaces;
using Cloudy.Infrastructure.Services;
using Cloudy.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using CloudyFile = Cloudy.Domain.Entities.File;

namespace Cloudy.Infrastructure.Tests.Services;

public class FileServiceTests
{
    private readonly Mock<IFileRepository> _fileRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private FileService CreateSut() => new(_fileRepo.Object, _uow.Object);

    [Fact]
    public async Task UploadAsync_Saves_And_Returns_Dto()
    {
        // Arrange
        var sut = CreateSut();
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var dto = await sut.UploadAsync("a.txt", stream, "text/plain");

        // Assert
        _fileRepo.Verify(r => r.AddAsync(It.IsAny<CloudyFile>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);

        dto.Name.Should().Be("a.txt");
        dto.Size.Should().Be(3);
        dto.ContentType.Should().Be("text/plain");
    }

    [Fact]
    public async Task RenameAsync_Updates_And_Saves()
    {
        // Arrange
        var file = new CloudyFile("old.txt", 1, new FileMetadata("text/plain", DateTime.UtcNow));
        _fileRepo.Setup(r => r.GetByIdAsync(file.Id)).ReturnsAsync(file);
        var sut = CreateSut();

        // Act
        await sut.RenameAsync(file.Id, "new.txt");

        // Assert
        file.Name.Should().Be("new.txt");
        _fileRepo.Verify(r => r.Update(file), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}

