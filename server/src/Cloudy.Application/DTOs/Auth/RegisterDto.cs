namespace Cloudy.Application.DTOs;

public class RegisterDto
{
    public string Username { get; private set; }
    public string Password { get; private set; }

    public RegisterDto(string username, string password)
    {
        Username = username;
        Password = password;
    }
}