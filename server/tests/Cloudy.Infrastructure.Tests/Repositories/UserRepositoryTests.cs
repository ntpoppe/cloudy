using Cloudy.Domain.Entities;
using Cloudy.Infrastructure.Data;
using Cloudy.Infrastructure.Repositories;
using Cloudy.Infrastructure.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Cloudy.Infrastructure.Tests.Repositories;

public class UserRepositoryTests
{
    private CloudyDbContext GetContext([System.Runtime.CompilerServices.CallerMemberName]string name = "")
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
    public async Task GetByIdAsync_Should_Return_User_When_Exists()
    {
        var ctx = GetContext();
        var repo = new UserRepository(ctx);
        var user = new User("bob", "bob@example.com", "pw");
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
}
