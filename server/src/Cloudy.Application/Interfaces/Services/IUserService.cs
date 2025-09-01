using Cloudy.Application.DTOs;

namespace Cloudy.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<UserDto?> AuthenticateAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}