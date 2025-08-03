using Cloudy.Domain.Entities;

namespace Cloudy.Application.Interfaces;

public interface IUserService
{
    Task<User>  RegisterAsync(string userName, string password);
    Task<User?> ValidateCredentialsAsync(string userName, string password);
}