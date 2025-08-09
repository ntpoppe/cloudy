using Cloudy.Application.DTOs;
using Cloudy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cloudy.API.Controllers;

[ApiController]
[Route("auth/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;

    public AuthController(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthenticationResponseDto>> Register(RegisterDto dto)
    {
        var user = await _userService.RegisterAsync(dto);
        var token = _jwtService.CreateToken(user.Id, user.Username);

        var userDto = new UserDto(user.Id, user.Username, user.Email);
        return Ok(new AuthenticationResponseDto(token, userDto));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponseDto>> Login(LoginDto dto)
    {
        var user = await _userService.AuthenticateAsync(dto);
        if (user is null)
            return Unauthorized();

        var token = _jwtService.CreateToken(user.Id, user.Username);
        var userDto = new UserDto(user.Id, user.Username, user.Email);
        return Ok(new AuthenticationResponseDto(token, userDto));
    }
}