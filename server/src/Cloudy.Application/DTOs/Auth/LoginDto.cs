namespace Cloudy.Application.DTOs;

public class LoginDto
{
    public string Username { get; private set; }
    public string Password { get; private set; }

    public LoginDto(string username, string password)
    {
        Username = username;
        Password = password;
    }
}