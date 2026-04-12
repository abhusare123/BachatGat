namespace BachatGat.Application.DTOs;

public record SendOtpRequest(string PhoneNumber);

public record VerifyOtpRequest(string PhoneNumber, string Otp, string FullName);

public record AuthResponse(string AccessToken, string RefreshToken, int UserId, string FullName, string PhoneNumber);

public record RefreshTokenRequest(string RefreshToken);
