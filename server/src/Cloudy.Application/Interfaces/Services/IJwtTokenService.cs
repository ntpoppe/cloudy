namespace Cloudy.Application.Interfaces;

public interface IJwtService 
{
    string CreateToken(int userId, string userName);
}