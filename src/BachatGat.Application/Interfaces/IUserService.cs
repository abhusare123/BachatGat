using BachatGat.Application.DTOs;

namespace BachatGat.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateUserProfileRequest request);

    /// <summary>Admin-only: get any user's profile.</summary>
    Task<UserProfileDto> GetProfileByIdAsync(int requestingUserId, int targetUserId);

    /// <summary>Admin-only: update any user's profile.</summary>
    Task<UserProfileDto> UpdateProfileByIdAsync(int requestingUserId, int targetUserId, UpdateUserProfileRequest request);

    /// <summary>Set or update the PIN for the current user.</summary>
    Task UpdatePinAsync(int userId, UpdatePinRequest request);
}
