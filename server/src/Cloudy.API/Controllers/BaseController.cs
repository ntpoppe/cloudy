using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Cloudy.API.Controllers;

public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the current authenticated user's ID from JWT claims.
    /// </summary>
    /// <returns>The user ID as an integer.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated or user ID cannot be determined.</exception>
    protected virtual int GetCurrentUserId()
    {
        if (User == null)
            throw new UnauthorizedAccessException("User not authenticated");

        string? userIdValue = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdValue) || !int.TryParse(userIdValue, out int userId))
            throw new UnauthorizedAccessException("User not authenticated");

        return userId;
    }
}

