using Cloudy.Application.Interfaces;
using Cloudy.Application.Services;
using Cloudy.Domain.Entities;
using FluentAssertions;
using Moq;

public class FolderServiceTests
{
    private readonly Mock<IFolderRepository> _folderRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private FolderService CreateSut() => new(_folderRepo.Object, _uow.Object);

    [Fact]
    public async Task CreateAsync_Persists_And_Maps_Dto()
    {
        // Arrange
        var sut = CreateSut();
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var dto = await sut.CreateAsync("Root");

        // Assert
        _folderRepo.Verify(r => r.AddAsync(It.IsAny<Folder>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        dto.Name.Should().Be("Root");
    }

    [Fact]
    public async Task ListAsync_Returns_Dtos()
    {
        // Arrange
        var parent = new Folder("P");
        var kids = new[] { new Folder("A", parent.Id), new Folder("B", parent.Id) };
        _folderRepo.Setup(r => r.ListByParentAsync(parent.Id))
                   .ReturnsAsync(kids);
        var sut = CreateSut();

        // Act
        var list = (await sut.ListAsync(parent.Id)).ToList();

        // Assert
        list.Should().HaveCount(2)
                     .And.OnlyContain(f => f.ParentFolderId == parent.Id);
    }
}
