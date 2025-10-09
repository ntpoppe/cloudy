using Cloudy.Domain.Entities.Bases;

namespace Cloudy.Domain.Entities;

public class User : TrackableEntity
{
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public string Email { get; private set; }

    // EF
    public User()
    {
        Username = null!;
        PasswordHash = null!;
        Email = null!;
    }

    public User(string username, string passwordHash, string email)
    {
        Username = username;
        PasswordHash = passwordHash;
        Email = email;
    }
}