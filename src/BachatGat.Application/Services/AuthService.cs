using System.Security.Cryptography;
using System.Text;
using BachatGat.Application.Abstractions;
using BachatGat.Application.DTOs;
using BachatGat.Application.Interfaces;
using BachatGat.Core.Entities;
using BachatGat.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BachatGat.Application.Services;

public class AuthService(IAppDbContext db, ISmsService sms, IJwtService jwt, IFirebaseTokenValidator firebaseValidator, ILogger<AuthService> logger) : IAuthService
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

        logger.LogInformation("OTP sent to {PhoneNumber}", phoneNumber);
    }

    public Task<bool> PhoneExistsAsync(string phoneNumber) =>
        db.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);

    public async Task<AuthResponse?> VerifyOtpAsync(string phoneNumber, string otp, string? fullName)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        bool isNew = user == null;

        // New user must supply a name before we consume the OTP
        if (isNew && string.IsNullOrWhiteSpace(fullName))
        {
            logger.LogWarning("OTP verify rejected for new user {PhoneNumber} — full name not provided", phoneNumber);
            return null;
        }

        var otpRecord = await db.OtpCodes
            .Where(o => o.PhoneNumber == phoneNumber && o.Code == otp && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otpRecord == null)
        {
            logger.LogWarning("OTP verification failed for {PhoneNumber} — invalid or expired code", phoneNumber);
            return null;
        }

        otpRecord.IsUsed = true;

        if (user == null)
        {
            user = new User { PhoneNumber = phoneNumber, FullName = fullName! };
            db.Users.Add(user);
        }
        else if (!string.IsNullOrWhiteSpace(fullName))
        {
            user.FullName = fullName;
        }

        await db.SaveChangesAsync();

        if (isNew)
            logger.LogInformation("New user registered via OTP — UserId {UserId}, Phone {PhoneNumber}", user.Id, phoneNumber);
        else
            logger.LogInformation("User {UserId} authenticated via OTP", user.Id);

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        var stored = await db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow);

        if (stored == null)
        {
            logger.LogWarning("Refresh token validation failed — token not found, revoked, or expired");
            return null;
        }

        // Revoke the used token (rotation — one-time use)
        stored.IsRevoked = true;
        await db.SaveChangesAsync();

        logger.LogInformation("Refresh token rotated for UserId {UserId}", stored.User.Id);
        return await IssueTokensAsync(stored.User);
    }

    public async Task<AuthResponse?> LoginAsync(string phoneNumber)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user == null)
        {
            logger.LogWarning("Login attempted for unregistered phone {PhoneNumber}", phoneNumber);
            return null;
        }

        logger.LogInformation("User {UserId} logged in via direct login", user.Id);
        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse?> FirebaseLoginAsync(string idToken)
    {
        var info = await firebaseValidator.ValidateAsync(idToken);
        if (info == null) return null;

        var user = await db.Users.FirstOrDefaultAsync(u => u.FirebaseUid == info.Uid);

        if (user == null && info.PhoneNumber != null)
            user = await db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == info.PhoneNumber);

        if (user == null && info.Email != null)
            user = await db.Users.FirstOrDefaultAsync(u => u.Email != null && u.Email == info.Email);

        if (user == null)
        {
            user = new User
            {
                FirebaseUid = info.Uid,
                PhoneNumber = info.PhoneNumber,
                Email = info.Email,
                FullName = info.Name ?? info.PhoneNumber ?? "User"
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            logger.LogInformation("New user registered via Firebase ({Provider}) — UserId {UserId}", info.SignInProvider, user.Id);
        }
        else
        {
            if (user.FirebaseUid == null)
            {
                user.FirebaseUid = info.Uid;
                await db.SaveChangesAsync();
            }
            logger.LogInformation("User {UserId} authenticated via Firebase ({Provider})", user.Id, info.SignInProvider);
        }

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
