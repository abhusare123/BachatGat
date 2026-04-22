using BachatGat.Application.DTOs;
using BachatGat.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BachatGat.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("firebase")]
    public async Task<IActionResult> FirebaseLogin([FromBody] FirebaseLoginRequest request)
    {
        var result = await authService.FirebaseLoginAsync(request.IdToken);
        if (result == null) return Unauthorized(new { Message = "Invalid Firebase token." });
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await authService.RefreshAsync(request.RefreshToken);
        if (result == null) return Unauthorized();
        return Ok(result);
    }

    [HttpPost("register-pin")]
    public async Task<IActionResult> RegisterWithPin([FromBody] RegisterWithPinRequest request)
    {
        var result = await authService.RegisterWithPinAsync(request);
        if (result == null)
            return Conflict(new { Message = "This phone number is already registered." });
        return Ok(result);
    }

    [HttpPost("login-pin")]
    public async Task<IActionResult> LoginWithPin([FromBody] LoginWithPinRequest request)
    {
        var result = await authService.LoginWithPinAsync(request);
        if (result == null)
            return Unauthorized(new { Message = "Invalid phone number or PIN." });
        return Ok(result);
    }
}
