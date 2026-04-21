using BachatGat.Application.DTOs;
using BachatGat.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BachatGat.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        await authService.SendOtpAsync(request.PhoneNumber);
        return Ok(new { Message = "OTP sent" });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var result = await authService.VerifyOtpAsync(request.PhoneNumber, request.Otp, request.FullName);
        if (result == null) return BadRequest(new { Message = "Invalid or expired OTP. New users must provide a name." });
        return Ok(result);
    }

    [HttpGet("check-phone")]
    public async Task<IActionResult> CheckPhone([FromQuery] string phone)
        => Ok(new { exists = await authService.PhoneExistsAsync(phone) });

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

    /// <summary>
    /// Direct login for pre-registered users.
    /// The admin adds the user's phone number to the database;
    /// the user can then log in immediately without an OTP.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request.PhoneNumber);
        if (result == null)
            return Unauthorized(new { Message = "Phone number not registered. Contact your group admin to be added." });
        return Ok(result);
    }
}
