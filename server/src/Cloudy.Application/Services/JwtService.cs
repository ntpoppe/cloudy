using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cloudy.Application.Interfaces.Services;
using Microsoft.IdentityModel.Tokens;

namespace Cloudy.Application.Services;

public class JwtService : IJwtService
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    public JwtService(string key, string issuer, string audience, int expiryMinutes)
    {
        _key = key;
        _issuer = issuer;
        _audience = audience;
        _expiryMinutes = expiryMinutes;
    }

    public string CreateToken(int userId, string userName)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, userName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

