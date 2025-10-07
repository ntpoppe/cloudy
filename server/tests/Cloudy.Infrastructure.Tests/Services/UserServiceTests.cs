using Cloudy.Application.DTOs;
using Cloudy.Application.Interfaces;
using Cloudy.Domain.Entities;
using Cloudy.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Cloudy.Infrastructure.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly Mock<IPasswordHasher<User>> _hasherMock = new();

    private UserService CreateService()
        => new UserService(_repoMock.Object, _hasherMock.Object);

    [Fact]
    public async Task RegisterAsync_Should_Create_User_And_Return_Dto()
    {
        // Arrange
        var dto = new RegisterDto("alice", "alice@example.com", "pw123");
        _repoMock.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _hasherMock.Setup(h => h.HashPassword(It.IsAny<User>(), dto.Password)).Returns("hashedpw");
        _repoMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(dto.Username);
        _repoMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == dto.Email && u.PasswordHash == "hashedpw"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_Should_Throw_If_Email_Exists()
    {
        // Arrange
        var dto = new RegisterDto("bob", "bob@example.com", "pw");
        _repoMock.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(new User("bob", "hash", "bob@example.com"));
        var service = CreateService();

        // Act
        Func<Task> act = () => service.RegisterAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Email already in use*");
        _repoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_Should_Return_Dto_On_Success()
    {
        // Arrange
        var dto = new LoginDto("eve", "pw");
        var user = new User("eve", "hashed", "eve@example.com");
        _repoMock.Setup(r => r.GetByUsernameOrEmailAsync(dto.UsernameOrEmail, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, dto.Password))
                   .Returns(PasswordVerificationResult.Success);

        var service = CreateService();

        // Act
        var result = await service.AuthenticateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(dto.UsernameOrEmail);
    }

    [Fact]
    public async Task AuthenticateAsync_Should_Return_Null_If_User_Not_Found()
    {
        // Arrange
        var dto = new LoginDto("notfound", "pw");
        _repoMock.Setup(r => r.GetByUsernameOrEmailAsync(dto.UsernameOrEmail, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var service = CreateService();

        // Act
        var result = await service.AuthenticateAsync(dto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_Should_Return_Null_If_Password_Invalid()
    {
        // Arrange
        var dto = new LoginDto("frank", "wrongpw");
        var user = new User("frank", "hashed", "frank@example.com");
        _repoMock.Setup(r => r.GetByUsernameOrEmailAsync(dto.UsernameOrEmail, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, dto.Password))
                   .Returns(PasswordVerificationResult.Failed);

        var service = CreateService();

        // Act
        var result = await service.AuthenticateAsync(dto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Dto_If_User_Exists()
    {
        // Arrange
        var user = new User("gina", "hash", "gina@example.com");
        typeof(User).GetProperty("Id")!.SetValue(user, 42); // ew
        _repoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_If_User_Not_Found()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(123);

        // Assert
        result.Should().BeNull();
    }
}
