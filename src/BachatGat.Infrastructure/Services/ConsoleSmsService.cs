using BachatGat.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BachatGat.Infrastructure.Services;

/// <summary>
/// Development stub — prints OTP to logs instead of sending an SMS.
/// Replace with Twilio/MSG91 implementation for production.
/// </summary>
public class ConsoleSmsService(ILogger<ConsoleSmsService> logger) : ISmsService
{
    public Task SendOtpAsync(string phoneNumber, string otp)
    {
        logger.LogWarning("=== [SMS STUB] OTP for {PhoneNumber}: {Otp} ===", phoneNumber, otp);
        return Task.CompletedTask;
    }
}
