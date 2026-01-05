using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Flashcards.Api.Features.Users;
using Microsoft.IdentityModel.Tokens;

namespace Flashcards.Api.Common.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;
    private readonly JwtSecurityTokenHandler _handler = new();

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(7);

    public string GenerateAccessToken(User user)
    {
        var key = _config["Jwt:Key"] 
                  ?? throw new InvalidOperationException("Jwt:Key not configured");
        var issuer = _config["Jwt:Issuer"] ?? "Flashcards.Api";
        var audience = _config["Jwt:Audience"] ?? "Flashcards.Client";

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return _handler.WriteToken(token);
    }
    
    public string GenerateRefreshToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
