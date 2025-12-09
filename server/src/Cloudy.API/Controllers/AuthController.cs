using Cloudy.Application.DTOs;
using Cloudy.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cloudy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IUserService userService, 
    IJwtService jwtService
) : BaseController
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
        var userId = GetCurrentUserId();
        var user = await userService.GetByIdAsync(userId);
        if (user is null)
            return NotFound();

        return Ok(user);
    }
}