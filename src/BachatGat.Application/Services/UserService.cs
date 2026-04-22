using BachatGat.Application.Abstractions;
using BachatGat.Application.DTOs;
using BachatGat.Application.Exceptions;
using BachatGat.Application.Interfaces;
using BachatGat.Core.Entities;
using BachatGat.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace BachatGat.Application.Services;

public class UserService(IAppDbContext db) : IUserService
{
    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new NotFoundException($"User {userId} not found");
        return ToDto(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateUserProfileRequest request)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new NotFoundException($"User {userId} not found");

        if (request.PhoneNumber != null && request.PhoneNumber != user.PhoneNumber)
        {
            bool taken = await db.Users.AnyAsync(u => u.Id != userId && u.PhoneNumber == request.PhoneNumber);
            if (taken) throw new BadRequestException("This mobile number is already registered to another account.");
        }

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        user.Email = request.Email;
        user.Address = request.Address;

        await db.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task<UserProfileDto> GetProfileByIdAsync(int requestingUserId, int targetUserId)
    {
        await RequireAdminAsync(requestingUserId);
        var user = await db.Users.FindAsync(targetUserId)
            ?? throw new NotFoundException($"User {targetUserId} not found");
        return ToDto(user);
    }

    public async Task<UserProfileDto> UpdateProfileByIdAsync(int requestingUserId, int targetUserId, UpdateUserProfileRequest request)
    {
        await RequireAdminAsync(requestingUserId);
        var user = await db.Users.FindAsync(targetUserId)
            ?? throw new NotFoundException($"User {targetUserId} not found");

        if (request.PhoneNumber != null && request.PhoneNumber != user.PhoneNumber)
        {
            bool taken = await db.Users.AnyAsync(u => u.Id != targetUserId && u.PhoneNumber == request.PhoneNumber);
            if (taken) throw new BadRequestException("This mobile number is already registered to another account.");
        }

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        user.Email = request.Email;
        user.Address = request.Address;

        await db.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task UpdatePinAsync(int userId, UpdatePinRequest request)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new NotFoundException($"User {userId} not found");

        if (user.PinHash != null)
        {
            if (string.IsNullOrEmpty(request.CurrentPin) || !BCrypt.Net.BCrypt.Verify(request.CurrentPin, user.PinHash))
                throw new BadRequestException("Current PIN is incorrect.");
        }

        user.PinHash = BCrypt.Net.BCrypt.HashPassword(request.NewPin);
        await db.SaveChangesAsync();
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task RequireAdminAsync(int userId)
    {
        var isAdmin = await db.GroupMembers
            .AnyAsync(m => m.UserId == userId && m.Role == GroupMemberRole.Admin && m.IsActive);
        if (!isAdmin)
            throw new ForbiddenException("Only group admins can view or edit other users' profiles.");
    }

    private static UserProfileDto ToDto(User u) =>
        new(u.Id, u.FullName, u.PhoneNumber, u.Email, u.Address, u.CreatedAt, u.PinHash != null);
}
