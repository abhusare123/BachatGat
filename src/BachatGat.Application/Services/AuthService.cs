using BachatGat.Application.Abstractions;
using BachatGat.Application.DTOs;
using BachatGat.Application.Interfaces;
using BachatGat.Core.Entities;
using BachatGat.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BachatGat.Application.Services;

public class AuthService(IAppDbContext db, ISmsService sms, IJwtService jwt) : IAuthService
{
    public async Task SendOtpAsync(string phoneNumber)
    {
        var otp = Random.Shared.Next(100000, 999999).ToString();
        db.OtpCodes.Add(new OtpCode
        {
            PhoneNumber = phoneNumber,
            Code = otp,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });
        await db.SaveChangesAsync();
        await sms.SendOtpAsync(phoneNumber, otp);
    }

    public async Task<AuthResponse?> VerifyOtpAsync(string phoneNumber, string otp, string fullName)
    {
        var otpRecord = await db.OtpCodes
            .Where(o => o.PhoneNumber == phoneNumber && o.Code == otp && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otpRecord == null) return null;

        otpRecord.IsUsed = true;

        var user = await db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user == null)
        {
            user = new User { PhoneNumber = phoneNumber, FullName = fullName };
            db.Users.Add(user);
        }
        else if (!string.IsNullOrWhiteSpace(fullName))
        {
            user.FullName = fullName;
        }

        await db.SaveChangesAsync();

        var (accessToken, refreshToken) = jwt.GenerateTokens(user);
        return new AuthResponse(accessToken, refreshToken, user.Id, user.FullName, user.PhoneNumber);
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken)
    {
        var userId = jwt.ValidateRefreshToken(refreshToken);
        if (userId == null) return null;

        var user = await db.Users.FindAsync(userId.Value);
        if (user == null) return null;

        var (accessToken, newRefreshToken) = jwt.GenerateTokens(user);
        return new AuthResponse(accessToken, newRefreshToken, user.Id, user.FullName, user.PhoneNumber);
    }
}
