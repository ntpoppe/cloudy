using Cloudy.Application.DTOs;
using Cloudy.Application.Interfaces;
using Cloudy.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Cloudy.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher<User> _hasher;

    public UserService(IUserRepository repo, IPasswordHasher<User> hasher)
    {
        _repo   = repo;
        _hasher = hasher;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto dto)
    {
        if (await _repo.GetByEmailAsync(dto.Email) is not null)
            throw new InvalidOperationException("Email already in use.");

        var temp = new User(dto.Username, passwordHash: "", dto.Email);
        var hash = _hasher.HashPassword(temp, dto.Password);
        var user = new User(dto.Username, hash, dto.Email);

        await _repo.AddAsync(user);
        return new UserDto(user.Id, user.Username, user.Email);
    }

    public async Task<UserDto?> AuthenticateAsync(LoginDto dto)
    {
        var user = await _repo.GetByEmailAsync(dto.Email);
        if (user is null) return null;

        var ok = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (ok == PasswordVerificationResult.Failed) return null;

        return new UserDto(user.Id, user.Username, user.Email);
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _repo.GetByIdAsync(id);
        return user is null
            ? null
            : new UserDto(user.Id, user.Username, user.Email);
    }
}