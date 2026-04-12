using BachatGat.Application.Abstractions;
using BachatGat.Application.DTOs;
using BachatGat.Application.Exceptions;
using BachatGat.Application.Interfaces;
using BachatGat.Core.Entities;
using BachatGat.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BachatGat.Application.Services;

public class GroupService(IAppDbContext db, ILogger<GroupService> logger) : IGroupService
{
    public async Task<List<GroupDto>> GetGroupsAsync(int currentUserId)
    {
        return await db.GroupMembers
            .Where(m => m.UserId == currentUserId && m.IsActive)
            .Include(m => m.Group).ThenInclude(g => g.Members)
            .Select(m => new GroupDto(
                m.Group.Id, m.Group.Name, m.Group.Description,
                m.Group.MonthlyAmount, m.Group.InterestRatePercent,
                m.Group.CreatedAt, m.Group.Members.Count(x => x.IsActive)))
            .ToListAsync();
    }

    public async Task<GroupDto> CreateGroupAsync(CreateGroupRequest request, int currentUserId)
    {
        var group = new Group
        {
            Name = request.Name,
            Description = request.Description,
            MonthlyAmount = request.MonthlyAmount,
            InterestRatePercent = request.InterestRatePercent,
            CreatedByUserId = currentUserId
        };
        db.Groups.Add(group);
        await db.SaveChangesAsync();

        db.GroupMembers.Add(new GroupMember
        {
            GroupId = group.Id,
            UserId = currentUserId,
            Role = GroupMemberRole.Admin
        });
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Group created — GroupId {GroupId}, Name '{GroupName}', by UserId {UserId}",
            group.Id, group.Name, currentUserId);

        return new GroupDto(group.Id, group.Name, group.Description,
            group.MonthlyAmount, group.InterestRatePercent, group.CreatedAt, 1);
    }

    public async Task<GroupDetailDto> GetGroupAsync(int groupId, int currentUserId)
    {
        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive);
        if (membership == null) throw new NotFoundException();

        var group = await db.Groups
            .Include(g => g.Members.Where(m => m.IsActive))
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null) throw new NotFoundException();

        return new GroupDetailDto(
            group.Id, group.Name, group.Description,
            group.MonthlyAmount, group.InterestRatePercent, group.CreatedAt,
            group.Members.Select(m => new GroupMemberDto(
                m.Id, m.UserId, m.User.FullName, m.User.PhoneNumber, m.Role, m.JoinedAt, m.IsActive)));
    }

    public async Task UpdateGroupAsync(int groupId, UpdateGroupRequest request, int currentUserId)
    {
        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive);
        if (membership == null || membership.Role > GroupMemberRole.Treasurer)
            throw new ForbiddenException();

        var group = await db.Groups.FindAsync(groupId)
            ?? throw new NotFoundException();

        group.Name = request.Name;
        group.Description = request.Description;
        group.MonthlyAmount = request.MonthlyAmount;
        group.InterestRatePercent = request.InterestRatePercent;
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Group {GroupId} updated by UserId {UserId} — new name '{GroupName}'",
            groupId, currentUserId, request.Name);
    }

    public async Task AddMemberAsync(int groupId, AddMemberRequest request, int currentUserId)
    {
        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive);
        if (membership == null || membership.Role != GroupMemberRole.Admin)
            throw new ForbiddenException();

        var user = await db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
        bool newUser = user == null;

        if (user == null)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new BadRequestException("No user found with this phone number. Provide a name to create a new user account.");

            user = new User { PhoneNumber = request.PhoneNumber, FullName = request.FullName.Trim() };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            logger.LogInformation(
                "New user account created for phone {PhoneNumber} while adding to GroupId {GroupId} by UserId {UserId}",
                request.PhoneNumber, groupId, currentUserId);
        }

        var existing = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == user.Id);

        if (existing != null)
        {
            if (existing.IsActive) throw new ConflictException("User is already a member");
            existing.IsActive = true;
            existing.Role = request.Role;

            logger.LogInformation(
                "Member UserId {TargetUserId} reactivated in GroupId {GroupId} with role {Role} by UserId {UserId}",
                user.Id, groupId, request.Role, currentUserId);
        }
        else
        {
            db.GroupMembers.Add(new GroupMember { GroupId = groupId, UserId = user.Id, Role = request.Role });

            logger.LogInformation(
                "Member UserId {TargetUserId} added to GroupId {GroupId} with role {Role} by UserId {UserId}",
                user.Id, groupId, request.Role, currentUserId);
        }

        await db.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(int groupId, int memberId, int currentUserId)
    {
        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive);
        if (membership == null || membership.Role != GroupMemberRole.Admin)
            throw new ForbiddenException();

        var target = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.Id == memberId && m.GroupId == groupId)
            ?? throw new NotFoundException();

        target.IsActive = false;
        await db.SaveChangesAsync();

        logger.LogInformation(
            "MemberId {MemberId} (UserId {TargetUserId}) removed from GroupId {GroupId} by UserId {UserId}",
            memberId, target.UserId, groupId, currentUserId);
    }
}
