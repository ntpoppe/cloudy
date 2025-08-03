namespace Cloudy.Application.Interfaces;

using Cloudy.Domain.Entities;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
}