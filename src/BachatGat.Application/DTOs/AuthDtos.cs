using System.ComponentModel.DataAnnotations;

namespace BachatGat.Application.DTOs;


public record FirebaseLoginRequest([Required] string IdToken);

public record AuthResponse(string AccessToken, string RefreshToken, int UserId, string FullName, string? PhoneNumber);

public record RefreshTokenRequest(
    [Required]
    string RefreshToken);

public record RegisterWithPinRequest(
    [Required, RegularExpression(@"^\d{10,15}$", ErrorMessage = "PhoneNumber must be 10–15 digits")]
    string PhoneNumber,

    [Required, MaxLength(100)]
    string FullName,

    [Required, RegularExpression(@"^\d{4,6}$", ErrorMessage = "PIN must be 4–6 digits")]
    string Pin);

public record LoginWithPinRequest(
    [Required, RegularExpression(@"^\d{10,15}$", ErrorMessage = "PhoneNumber must be 10–15 digits")]
    string PhoneNumber,

    [Required, RegularExpression(@"^\d{4,6}$", ErrorMessage = "PIN must be 4–6 digits")]
    string Pin);
