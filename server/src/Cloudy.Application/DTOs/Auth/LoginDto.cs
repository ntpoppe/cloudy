namespace Cloudy.Application.DTOs;

public record LoginDto(
    string UsernameOrEmail,
    string Password
);