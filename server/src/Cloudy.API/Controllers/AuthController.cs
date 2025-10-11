using Cloudy.Application.DTOs;
using Cloudy.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Cloudy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IUserService userService, 
    IJwtService jwtService
) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthenticationResponseDto>> RegisterAsync(RegisterDto dto)
    {
        // Log entry to terminal
        Console.WriteLine("[AuthController] Register endpoint called");

        var user = await userService.RegisterAsync(dto);
        var token = jwtService.CreateToken(user.Id, user.Username);

        var userDto = new UserDto(user.Id, user.Username, user.Email);
        return Ok(new AuthenticationResponseDto(token, userDto));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await userService.AuthenticateAsync(dto);
        if (user is null)
            return Unauthorized();

        var token = jwtService.CreateToken(user.Id, user.Username);
        var userDto = new UserDto(user.Id, user.Username, user.Email);
        return Ok(new AuthenticationResponseDto(token, userDto));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> MeAsync()
    {
        string? userIdValue = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out int userId))
            return Unauthorized();

        var user = await userService.GetByIdAsync(userId);
        if (user is null)
            return NotFound();

        return Ok(user);
    }
}