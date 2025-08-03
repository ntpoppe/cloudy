namespace Cloudy.Domain.Entities;

public class User : Entity
{
    public string Username { get; private set; }
    public string Password { get; private set; } // hash this later

    public User(string username, string password)
    {
        Username = username;
        Password = password;
    }
}