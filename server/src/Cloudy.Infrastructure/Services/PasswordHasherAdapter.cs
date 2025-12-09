using Cloudy.Application.Interfaces.Services;
using Cloudy.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Cloudy.Infrastructure.Services;

public class PasswordHasherAdapter : IPasswordHasher
{
    private readonly IPasswordHasher<User> _identityHasher;

    public PasswordHasherAdapter(IPasswordHasher<User> identityHasher)
    {
        _identityHasher = identityHasher;
    }

    public string HashPassword(object user, string password)
    {
        if (user is not User domainUser)
            throw new ArgumentException("User must be of type Domain.Entities.User", nameof(user));
        
        return _identityHasher.HashPassword(domainUser, password);
    }

    public Application.Interfaces.Services.PasswordVerificationResult VerifyHashedPassword(object user, string hashedPassword, string providedPassword)
    {
        if (user is not User domainUser)
            throw new ArgumentException("User must be of type Domain.Entities.User", nameof(user));
        
        var result = _identityHasher.VerifyHashedPassword(domainUser, hashedPassword, providedPassword);
        
        return result switch
        {
            Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed => Application.Interfaces.Services.PasswordVerificationResult.Failed,
            Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success => Application.Interfaces.Services.PasswordVerificationResult.Success,
            Microsoft.AspNetCore.Identity.PasswordVerificationResult.SuccessRehashNeeded => Application.Interfaces.Services.PasswordVerificationResult.SuccessRehashNeeded,
            _ => Application.Interfaces.Services.PasswordVerificationResult.Failed
        };
    }
}

