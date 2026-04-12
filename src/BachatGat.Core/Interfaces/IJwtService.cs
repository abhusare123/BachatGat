using BachatGat.Core.Entities;

namespace BachatGat.Core.Interfaces;

public interface IJwtService
{
    (string AccessToken, string RefreshToken) GenerateTokens(User user);
    int? ValidateRefreshToken(string token);
}
