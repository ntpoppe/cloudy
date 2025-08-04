namespace Cloudy.Application.DTOs;

public record RegisterDto(
    string Username,
    string Email,
    string Password
);