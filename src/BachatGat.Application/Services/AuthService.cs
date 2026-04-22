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

public class AuthService(IAppDbContext db, IJwtService jwt, IFirebaseTokenValidator firebaseValidator, ILogger<AuthService> logger) : IAuthService
{
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

        stored.IsRevoked = true;
        await db.SaveChangesAsync();

        logger.LogInformation("Refresh token rotated for UserId {UserId}", stored.User.Id);
        return await IssueTokensAsync(stored.User);
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

    public async Task<AuthResponse?> RegisterWithPinAsync(RegisterWithPinRequest request)
    {
        bool exists = await db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (exists)
        {
            logger.LogWarning("PIN registration rejected — phone {PhoneNumber} already registered", request.PhoneNumber);
            return null;
        }

        var user = new User
        {
            PhoneNumber = request.PhoneNumber,
            FullName = request.FullName,
            PinHash = BCrypt.Net.BCrypt.HashPassword(request.Pin)
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        logger.LogInformation("New user registered via PIN — UserId {UserId}, Phone {PhoneNumber}", user.Id, request.PhoneNumber);
        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse?> LoginWithPinAsync(LoginWithPinRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (user == null || user.PinHash == null || !BCrypt.Net.BCrypt.Verify(request.Pin, user.PinHash))
        {
            logger.LogWarning("PIN login failed for {PhoneNumber}", request.PhoneNumber);
            return null;
        }

        logger.LogInformation("User {UserId} authenticated via PIN", user.Id);
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
