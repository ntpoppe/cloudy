namespace Cloudy.Application.Interfaces;

public interface IJwtTokenService 
{
    string CreateToken(int userId, string userName);
}