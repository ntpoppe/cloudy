using Cloudy.Application.DTOs;

public record AuthenticationResponseDto(
    string Token,
    UserDto User
);