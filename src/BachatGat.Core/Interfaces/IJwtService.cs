using BachatGat.Core.Entities;

namespace BachatGat.Core.Interfaces;

public interface IJwtService
{
    /// <summary>Returns a JWT access token, an opaque refresh token, and the refresh token's expiry.</summary>
    (string AccessToken, string RefreshToken, DateTime RefreshExpiresAt) GenerateTokens(User user);
}
