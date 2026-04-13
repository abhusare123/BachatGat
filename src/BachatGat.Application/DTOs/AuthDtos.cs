using System.ComponentModel.DataAnnotations;

namespace BachatGat.Application.DTOs;

public record SendOtpRequest(
    [Required, RegularExpression(@"^\d{10,15}$", ErrorMessage = "PhoneNumber must be 10–15 digits")]
    string PhoneNumber);

public record VerifyOtpRequest(
    [Required, RegularExpression(@"^\d{10,15}$", ErrorMessage = "PhoneNumber must be 10–15 digits")]
    string PhoneNumber,

    [Required, RegularExpression(@"^\d{6}$", ErrorMessage = "Otp must be exactly 6 digits")]
    string Otp,

    [Required, MaxLength(100)]
    string FullName);

/// <summary>Direct login for pre-registered users (no OTP required).</summary>
public record LoginRequest(
    [Required, RegularExpression(@"^\d{10,15}$", ErrorMessage = "PhoneNumber must be 10–15 digits")]
    string PhoneNumber);

public record AuthResponse(string AccessToken, string RefreshToken, int UserId, string FullName, string PhoneNumber);

public record RefreshTokenRequest(
    [Required]
    string RefreshToken);
