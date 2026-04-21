using System.ComponentModel.DataAnnotations;

namespace BachatGat.Application.DTOs;

public record UserProfileDto(
    int Id,
    string FullName,
    string? PhoneNumber,
    string? Email,
    string? Address,
    DateTime CreatedAt);

public record UpdateUserProfileRequest(
    [Required, MaxLength(100)]
    string FullName,

    [MaxLength(200), EmailAddress]
    string? Email,

    [MaxLength(500)]
    string? Address);
