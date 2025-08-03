using Cloudy.Application.DTOs;

namespace Cloudy.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterAsync(RegisterDto dto);
    Task<UserDto?> AuthenticateAsync(LoginDto dto);
    Task<UserDto?> GetByIdAsync(int id);
}