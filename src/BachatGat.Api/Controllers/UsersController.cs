using System.Security.Claims;
using BachatGat.Application.DTOs;
using BachatGat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BachatGat.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IUserService userService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get the currently logged-in user's profile.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
        => Ok(await userService.GetProfileAsync(CurrentUserId));

    /// <summary>Update the currently logged-in user's profile.</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileRequest request)
        => Ok(await userService.UpdateProfileAsync(CurrentUserId, request));

    /// <summary>Admin-only: get any user's profile.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserProfile(int id)
        => Ok(await userService.GetProfileByIdAsync(CurrentUserId, id));

    /// <summary>Admin-only: update any user's profile.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUserProfile(int id, [FromBody] UpdateUserProfileRequest request)
        => Ok(await userService.UpdateProfileByIdAsync(CurrentUserId, id, request));
}
