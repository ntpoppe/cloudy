using Cloudy.Domain.ValueObjects;
using FluentAssertions;

namespace Cloudy.Domain.Tests.ValueObjects;

public class FileMetadataTests
{
    [Fact]
    public void Ctor_Should_Set_Properties()
    {
        var dt = DateTime.UtcNow;
        var metadata = new FileMetadata("image/png", dt);

        metadata.ContentType.Should().Be("image/png");
        metadata.UploadedAt.Should().Be(dt);
    }

    [Fact]
    public void Equals_SameValues_ShouldBeTrue()
    {
        var dt = DateTime.UtcNow;
        var a = new FileMetadata("application/pdf", dt);
        var b = new FileMetadata("application/pdf", dt);

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentContentType_ShouldBeFalse()
    {
        var dt = DateTime.UtcNow;
        var a = new FileMetadata("application/json", dt);
        var b = new FileMetadata("application/xml", dt);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentTimestamp_ShouldBeFalse()
    {
        var a = new FileMetadata("text/plain", DateTime.UtcNow);
        var b = new FileMetadata("text/plain", DateTime.UtcNow.AddMinutes(-5));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_Should_WorkCorrectly()
    {
        var dt = DateTime.UtcNow;
        var a = new FileMetadata("text/html", dt);
        var b = new FileMetadata("text/html", dt);

        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }
}