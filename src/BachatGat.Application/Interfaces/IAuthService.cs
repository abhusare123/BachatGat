using BachatGat.Application.DTOs;

namespace BachatGat.Application.Interfaces;

public interface IAuthService
{
    Task SendOtpAsync(string phoneNumber);

    /// <returns>AuthResponse on success, null if OTP is invalid/expired.</returns>
    Task<AuthResponse?> VerifyOtpAsync(string phoneNumber, string otp, string fullName);

    /// <returns>AuthResponse on success, null if refresh token is invalid.</returns>
    Task<AuthResponse?> RefreshAsync(string refreshToken);
}
