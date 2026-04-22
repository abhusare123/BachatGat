using BachatGat.Application.DTOs;

namespace BachatGat.Application.Interfaces;

public interface IAuthService
{
    Task SendOtpAsync(string phoneNumber);

    /// <returns>AuthResponse on success, null if OTP is invalid/expired.</returns>
    Task<AuthResponse?> VerifyOtpAsync(string phoneNumber, string otp, string? fullName);
    Task<bool> PhoneExistsAsync(string phoneNumber);

    /// <returns>AuthResponse on success, null if refresh token is invalid.</returns>
    Task<AuthResponse?> RefreshAsync(string refreshToken);

    /// <summary>
    /// Direct login for pre-registered users — no OTP required.
    /// The admin adds users to the database; this lets them log in immediately.
    /// </summary>
    /// <returns>AuthResponse if phone number is registered, null otherwise.</returns>
    Task<AuthResponse?> LoginAsync(string phoneNumber);

    /// <returns>AuthResponse on success, null if the Firebase ID token is invalid.</returns>
    Task<AuthResponse?> FirebaseLoginAsync(string idToken);

    /// <returns>AuthResponse on success, null if phone already registered.</returns>
    Task<AuthResponse?> RegisterWithPinAsync(RegisterWithPinRequest request);

    /// <returns>AuthResponse on success, null if credentials are invalid.</returns>
    Task<AuthResponse?> LoginWithPinAsync(LoginWithPinRequest request);
}
