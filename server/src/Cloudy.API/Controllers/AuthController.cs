using Cloudy.Application.DTOs;
using Cloudy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cloudy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtService;

    public AuthController(IUserService userService, IJwtTokenService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = await _userService.RegisterAsync(dto.Username, dto.Password);
        var token = _jwtService.CreateToken(user.Id, user.Username);
        return Ok(new { token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userService.ValidateCredentialsAsync(dto.Username, dto.Password);
        if (user is null)
            return Unauthorized();
        var token = _jwtService.CreateToken(user.Id, user.Username);
        return Ok(new { token });
    }
}