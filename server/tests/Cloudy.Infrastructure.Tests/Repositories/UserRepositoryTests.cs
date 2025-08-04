using Cloudy.Domain.Entities;
using Cloudy.Infrastructure.Data;
using Cloudy.Infrastructure.Repositories;
using Cloudy.Infrastructure.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Cloudy.Infrastructure.Tests.Repositories;

public class UserRepositoryTests
{
    private CloudyDbContext GetContext([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        => InMemoryContextFactory.Create($"UserRepo_{name}");

    [Fact]
    public async Task AddAsync_Should_Persist_User()
    {
        var ctx = GetContext();
        var repo = new UserRepository(ctx);
        var user = new User("alice", "hashedpassword", "alice@example.com");

        await repo.AddAsync(user);

        var fetched = await ctx.Users.SingleAsync();
        fetched.Username.Should().Be("alice");
        fetched.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_Should_Return_User_When_Exists()
    {
        var ctx = GetContext();
        var repo = new UserRepository(ctx);
        var user = new User("bob", "hashedpw", "bob@example.com");
        await repo.AddAsync(user);

        var result = await repo.GetByEmailAsync("bob@example.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("bob@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_Should_Return_Null_When_NotFound()
    {
        var ctx = GetContext();
        var repo = new UserRepository(ctx);

        var result = await repo.GetByEmailAsync("notfound@example.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_User_When_Exists()
    {
        var ctx = GetContext();
        var repo = new UserRepository(ctx);
        var user = new User("carol", "pw", "carol@example.com");
        await repo.AddAsync(user);

        var result = await repo.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
    {
        var ctx = GetContext();
        var repo = new UserRepository(ctx);

        var result = await repo.GetByIdAsync(-1);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_Should_Return_User_When_Exists()
    {
        var ctx = GetContext();
        var repo = new UserRepository(ctx);
        var user = new User("dave", "pw", "dave@example.com");
        await repo.AddAsync(user);

        var result = await repo.GetByUsernameAsync("dave");

        result.Should().NotBeNull();
        result!.Username.Should().Be("dave");
    }

    [Fact]
    public async Task GetByUsernameAsync_Should_Return_Null_When_NotFound()
    {
        var ctx = GetContext();
        var repo = new UserRepository(ctx);

        var result = await repo.GetByUsernameAsync("notfound");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameOrEmailAsync_Should_Return_User_By_Email()
    {
        var ctx = GetContext();
        var repo = new UserRepository(ctx);
        var user = new User("eve", "pw", "eve@example.com");
        await repo.AddAsync(user);

        var result = await repo.GetByUsernameOrEmailAsync("eve@example.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("eve@example.com");
    }

    [Fact]
    public async Task GetByUsernameOrEmailAsync_Should_Return_User_By_Username()
    {
        var ctx = GetContext();
        var repo = new UserRepository(ctx);
        var user = new User("frank", "pw", "frank@example.com");
        await repo.AddAsync(user);

        var result = await repo.GetByUsernameOrEmailAsync("frank");

        result.Should().NotBeNull();
        result!.Username.Should().Be("frank");
    }

    [Fact]
    public async Task GetByUsernameOrEmailAsync_Should_Return_Null_When_NotFound()
    {
        var ctx = GetContext();
        var repo = new UserRepository(ctx);

        var result = await repo.GetByUsernameOrEmailAsync("notfound");

        result.Should().BeNull();
    }
}
