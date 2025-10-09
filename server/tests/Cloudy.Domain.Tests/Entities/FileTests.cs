using Cloudy.Domain.Exceptions;
using Cloudy.Domain.ValueObjects;
using FluentAssertions;
using File = Cloudy.Domain.Entities.File;

namespace Cloudy.Domain.Tests.Entities;

public class FileTests
{
    [Fact]
    public void Ctor_Should_Set_Properties()
    {
        // Arrange
        var metadata = new FileMetadata("image/png", DateTime.UtcNow);
        var userId = 1;

        // Act
        var file = new File("pic.png", 123, metadata, userId);

        // Assert
        file.Name.Should().Be("pic.png");
        file.Size.Should().Be(123);
        file.Metadata.Should().Be(metadata);
        file.CreatedBy.Should().Be(userId);
        file.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Rename_ValidName_Should_UpdateName()
    {
        // Arrange
        var file = new File("old.txt", 10, new FileMetadata("text/plain", DateTime.UtcNow), 1);
        var newName = "new.txt";

        // Act
        file.Rename(newName, 1);

        // Assert
        file.Name.Should().Be("new.txt");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_InvalidName_Should_Throw(string? badName)
    {
        // Arrange
        var file = new File("a.txt", 5, new FileMetadata("text/plain", DateTime.UtcNow), 1);

        // Act
        Action act = () => file.Rename(badName, 1);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Name cannot be empty.");
    }

    [Fact]
    public void SoftDelete_Should_Set_Flags()
    {
        // Arrange
        var file = new File("t.txt", 1, new FileMetadata("text/plain", DateTime.UtcNow), 1);

        // Act
        file.SoftDelete(1);

        // Assert
        file.IsDeleted.Should().BeTrue();
        file.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}