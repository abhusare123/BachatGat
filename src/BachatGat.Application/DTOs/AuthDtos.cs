namespace BachatGat.Application.DTOs;

public record SendOtpRequest(string PhoneNumber);

public record VerifyOtpRequest(string PhoneNumber, string Otp, string FullName);

/// <summary>Direct login for pre-registered users (no OTP required).</summary>
public record LoginRequest(string PhoneNumber);

public record AuthResponse(string AccessToken, string RefreshToken, int UserId, string FullName, string PhoneNumber);

public record RefreshTokenRequest(string RefreshToken);
