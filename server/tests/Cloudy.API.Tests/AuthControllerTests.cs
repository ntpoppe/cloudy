using System.Reflection;
using Cloudy.API.Controllers;
using Cloudy.Application.DTOs;
using Cloudy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class AuthControllerTests
{
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_userService.Object, _jwtService.Object);
    }

    [Fact]
    public async Task Register_Returns_Ok_With_Token()
    {
        // Arrange
        var dto = new RegisterDto("testuser", "pass123", "test@example.com");
        var userDto = new UserDto(1, "testuser", "test@example.com");

        _userService
            .Setup(s => s.RegisterAsync(dto))
            .ReturnsAsync(userDto);

        _jwtService
            .Setup(s => s.CreateToken(userDto.Id, userDto.Username))
            .Returns("expected-token");

        // Act
        var actionResult = await _controller.Register(dto);

        // Assert
        var result = Assert.IsType<ActionResult<AuthenticationResponseDto>>(actionResult);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthenticationResponseDto>(okResult.Value);

        Assert.Equal("expected-token", response.Token);

        _userService.Verify(s => s.RegisterAsync(dto), Times.Once);
        _jwtService.Verify(s => s.CreateToken(userDto.Id, userDto.Username), Times.Once);
    }
}
