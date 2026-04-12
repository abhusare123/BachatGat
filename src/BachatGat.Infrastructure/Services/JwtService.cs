using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BachatGat.Core.Entities;
using BachatGat.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BachatGat.Infrastructure.Services;

public class JwtService(IConfiguration config) : IJwtService
{
    public (string AccessToken, string RefreshToken) GenerateTokens(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        var accessToken = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:ExpiryMinutes"]!)),
            signingCredentials: creds);

        var refreshClaims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
        var refreshToken = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: refreshClaims,
            expires: DateTime.UtcNow.AddDays(double.Parse(config["Jwt:RefreshExpiryDays"]!)),
            signingCredentials: creds);

        return (
            new JwtSecurityTokenHandler().WriteToken(accessToken),
            new JwtSecurityTokenHandler().WriteToken(refreshToken)
        );
    }

    public int? ValidateRefreshToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = config["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true
            }, out _);

            return int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }
        catch
        {
            return null;
        }
    }
}
