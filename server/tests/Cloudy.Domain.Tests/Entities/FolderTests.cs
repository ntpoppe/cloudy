using Cloudy.Domain.Entities;
using Cloudy.Domain.Exceptions;
using FluentAssertions;

namespace Cloudy.Domain.Tests.Entities;

public class FolderTests
{
    [Fact]
    public void Ctor_Should_Set_Properties()
    {
        // Arrange & Act
        var folder = new Folder("Docs");

        // Assert
        folder.Name.Should().Be("Docs");
        folder.ParentFolderId.Should().BeNull();
        folder.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Ctor_WithParentId_Should_LinkParent()
    {
        // Arrange
        var parent = new Folder("Root");

        // Act
        var child = new Folder("Child", parent.Id);

        // Assert
        child.ParentFolderId.Should().Be(parent.Id);
    }

    [Fact]
    public void Rename_WithValidName_Should_Update()
    {
        // Arrange
        var folder = new Folder("Old");

        // Act
        folder.Rename("New");

        // Assert
        folder.Name.Should().Be("New");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithInvalidName_ShouldThrow(string? badName)
    {
        // Arrange
        var folder = new Folder("Valid");

        // Act
        Action act = () => folder.Rename(badName);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Name cannot be empty.");
    }

    [Fact]
    public void SoftDelete_Should_Mark_AsDeleted()
    {
        // Arrange
        var folder = new Folder("Archive");

        // Act
        folder.SoftDelete(1);

        // Assert
        folder.IsDeleted.Should().BeTrue();
        folder.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
