using System.Security.Cryptography;
using System.Text;
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

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        var stored = await db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow);

        if (stored == null) return null;

        // Revoke the used token (rotation — one-time use)
        stored.IsRevoked = true;
        await db.SaveChangesAsync();

        return await IssueTokensAsync(stored.User);
    }

    public async Task<AuthResponse?> LoginAsync(string phoneNumber)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user == null) return null;   // not registered — admin must add the user first

        return await IssueTokensAsync(user);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<AuthResponse> IssueTokensAsync(User user)
    {
        var (accessToken, refreshToken, refreshExpiresAt) = jwt.GenerateTokens(user);

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            ExpiresAt = refreshExpiresAt
        });
        await db.SaveChangesAsync();

        return new AuthResponse(accessToken, refreshToken, user.Id, user.FullName, user.PhoneNumber);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
