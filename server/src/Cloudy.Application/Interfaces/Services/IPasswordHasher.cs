namespace Cloudy.Application.Interfaces.Services;

public enum PasswordVerificationResult
{
    Failed,
    Success,
    SuccessRehashNeeded
}

public interface IPasswordHasher
{
    string HashPassword(object user, string password);
    PasswordVerificationResult VerifyHashedPassword(object user, string hashedPassword, string providedPassword);
}

