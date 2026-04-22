using System.ComponentModel.DataAnnotations;

namespace BachatGat.Application.DTOs;

public record UserProfileDto(
    int Id,
    string FullName,
    string? PhoneNumber,
    string? Email,
    string? Address,
    DateTime CreatedAt,
    bool HasPin);

public record UpdateUserProfileRequest(
    [Required, MaxLength(100)]
    string FullName,

    [RegularExpression(@"^\d{10,15}$", ErrorMessage = "PhoneNumber must be 10–15 digits")]
    string? PhoneNumber,

    [MaxLength(200), EmailAddress]
    string? Email,

    [MaxLength(500)]
    string? Address);

public record UpdatePinRequest(
    string? CurrentPin,

    [Required, RegularExpression(@"^\d{4,6}$", ErrorMessage = "PIN must be 4–6 digits")]
    string NewPin);
