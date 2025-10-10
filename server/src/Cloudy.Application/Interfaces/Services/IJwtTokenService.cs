namespace Cloudy.Application.Interfaces.Services;

public interface IJwtService 
{
    string CreateToken(int userId, string userName);
}