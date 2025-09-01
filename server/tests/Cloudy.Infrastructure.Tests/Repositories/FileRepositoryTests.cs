using Cloudy.Domain.ValueObjects;
using Cloudy.Infrastructure.Data;
using Cloudy.Infrastructure.Repositories;
using Cloudy.Infrastructure.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using CloudyFile = Cloudy.Domain.Entities.File;

namespace Cloudy.Infrastructure.Tests.Repositories;

public class FileRepositoryTests
{
    private CloudyDbContext GetContext([System.Runtime.CompilerServices.CallerMemberName]string name = "")
        => InMemoryContextFactory.Create($"FileRepo_{name}");

    [Fact]
    public async Task AddAsync_Should_Persist_File()
    {
        var ctx = GetContext();
        var repo = new FileRepository(ctx);
        var metadata = new FileMetadata("text/plain", DateTime.UtcNow);
        var file = new CloudyFile("hello.txt", 12, metadata, 1);

        await repo.AddAsync(file);
        await ctx.SaveChangesAsync();

        var fetched = await ctx.Files.SingleAsync();
        fetched.Name.Should().Be("hello.txt");
        fetched.Metadata.ContentType.Should().Be("text/plain");
        fetched.UserId.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_File_When_Exists()
    {
        var ctx = GetContext();
        var repo = new FileRepository(ctx);
        var file = new CloudyFile("a.txt", 5, new FileMetadata("text/plain", DateTime.UtcNow), 1);
        await repo.AddAsync(file);
        await ctx.SaveChangesAsync();

        var result = await repo.GetByIdAsync(file.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(file.Id);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
    {
        var ctx = GetContext();
        var repo = new FileRepository(ctx);

        var result = await repo.GetByIdAsync(-1);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Update_Should_Track_Modifications()
    {
        var ctx = GetContext();
        var repo = new FileRepository(ctx);
        var file = new CloudyFile("old.txt", 3, new FileMetadata("text/plain", DateTime.UtcNow), 1);
        await repo.AddAsync(file);
        await ctx.SaveChangesAsync();

        file.Rename("new.txt");
        repo.Update(file);
        await ctx.SaveChangesAsync();

        var updated = await ctx.Files.SingleAsync();
        updated.Name.Should().Be("new.txt");
    }

    [Fact]
    public async Task SoftDeleted_Files_Are_Filtered_Out()
    {
        var ctx = GetContext();
        var repo = new FileRepository(ctx);

        var keep = new CloudyFile("keep.txt", 1, new FileMetadata("text/plain", DateTime.UtcNow), 1);
        var del  = new CloudyFile("del.txt", 2, new FileMetadata("text/plain", DateTime.UtcNow), 1);
        del.SoftDelete();

        await repo.AddAsync(keep);
        await repo.AddAsync(del);
        await ctx.SaveChangesAsync();

        var all = await ctx.Files.ToListAsync();
        all.Should().ContainSingle(f => f.Name == "keep.txt");
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_Only_User_Files()
    {
        var ctx = GetContext();
        var repo = new FileRepository(ctx);

        var user1File = new CloudyFile("user1.txt", 1, new FileMetadata("text/plain", DateTime.UtcNow), 1);
        var user2File = new CloudyFile("user2.txt", 2, new FileMetadata("text/plain", DateTime.UtcNow), 2);

        await repo.AddAsync(user1File);
        await repo.AddAsync(user2File);
        await ctx.SaveChangesAsync();

        var user1Files = await repo.GetByUserIdAsync(1);
        user1Files.Should().ContainSingle(f => f.Name == "user1.txt");

        var user2Files = await repo.GetByUserIdAsync(2);
        user2Files.Should().ContainSingle(f => f.Name == "user2.txt");
    }
}
