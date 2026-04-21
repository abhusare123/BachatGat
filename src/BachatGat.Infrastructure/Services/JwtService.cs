using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BachatGat.Core.Entities;
using BachatGat.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BachatGat.Infrastructure.Services;

public class JwtService(IConfiguration config) : IJwtService
{
    public (string AccessToken, string RefreshToken, DateTime RefreshExpiresAt) GenerateTokens(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? ""),
            new Claim(ClaimTypes.Name, user.FullName ?? "")
        };

        var accessToken = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:ExpiryMinutes"]!)),
            signingCredentials: creds);

        // Opaque random refresh token — not a JWT, validated via DB lookup
        var refreshTokenBytes = RandomNumberGenerator.GetBytes(64);
        var refreshToken = Convert.ToBase64String(refreshTokenBytes);
        var refreshExpiresAt = DateTime.UtcNow.AddDays(double.Parse(config["Jwt:RefreshExpiryDays"]!));

        return (
            new JwtSecurityTokenHandler().WriteToken(accessToken),
            refreshToken,
            refreshExpiresAt
        );
    }
}
