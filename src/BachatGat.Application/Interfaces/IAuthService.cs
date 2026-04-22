using BachatGat.Application.DTOs;

namespace BachatGat.Application.Interfaces;

public interface IAuthService
{
    /// <returns>AuthResponse on success, null if refresh token is invalid.</returns>
    Task<AuthResponse?> RefreshAsync(string refreshToken);

    /// <returns>AuthResponse on success, null if the Firebase ID token is invalid.</returns>
    Task<AuthResponse?> FirebaseLoginAsync(string idToken);

    /// <returns>AuthResponse on success, null if phone already registered.</returns>
    Task<AuthResponse?> RegisterWithPinAsync(RegisterWithPinRequest request);

    /// <returns>AuthResponse on success, null if credentials are invalid.</returns>
    Task<AuthResponse?> LoginWithPinAsync(LoginWithPinRequest request);
}
