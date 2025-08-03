using Cloudy.Domain.Entities;
using Cloudy.Infrastructure.Data;
using Cloudy.Infrastructure.Repositories;
using Cloudy.Infrastructure.Tests.TestHelpers;
using FluentAssertions;

namespace Cloudy.Infrastructure.Tests.Repositories;

public class EfFolderRepositoryTests
{
    private CloudyDbContext GetContext([System.Runtime.CompilerServices.CallerMemberName]string name = "")
        => InMemoryContextFactory.Create($"FolderRepo_{name}");

    [Fact]
    public async Task AddAndGetById_Works()
    {
        var ctx = GetContext();
        var repo = new EfFolderRepository(ctx);
        var folder = new Folder("root");

        await repo.AddAsync(folder);
        await ctx.SaveChangesAsync();

        var fetched = await repo.GetByIdAsync(folder.Id);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("root");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
    {
        var ctx = GetContext();
        var repo = new EfFolderRepository(ctx);

        var result = await repo.GetByIdAsync(-1);
        result.Should().BeNull();
    }

    [Fact]
    public async Task ListByParentAsync_Returns_CorrectChildren()
    {
        // Arrange
        var ctx = GetContext();
        var repo = new EfFolderRepository(ctx);

        var parent = new Folder("p");
        await repo.AddAsync(parent);
        await ctx.SaveChangesAsync();

        var c1 = new Folder("c1", parent.Id);
        var c2 = new Folder("c2", parent.Id);
        var other = new Folder("x");

        await repo.AddAsync(c1);
        await repo.AddAsync(c2);
        await repo.AddAsync(other);
        await ctx.SaveChangesAsync();

        // Act
        var children = await repo.ListByParentAsync(parent.Id);

        // Assert
        children.Should()
                .HaveCount(2)
                .And.OnlyContain(f => f.ParentFolderId == parent.Id);
    }
}