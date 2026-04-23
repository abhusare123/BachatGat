using BachatGat.Application.Abstractions;
using BachatGat.Application.DTOs;
using BachatGat.Application.Exceptions;
using BachatGat.Application.Interfaces;
using BachatGat.Core.Entities;
using BachatGat.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BachatGat.Application.Services;

public class GroupIncomeService(IAppDbContext db, ILogger<GroupIncomeService> logger)
    : IGroupIncomeService
{
    public async Task<List<GroupIncomeDto>> GetEntriesAsync(int groupId, int currentUserId)
    {
        if (!await IsMemberAsync(groupId, currentUserId)) throw new NotFoundException();

        return await db.GroupIncomes
            .Include(e => e.RecordedBy)
            .Include(e => e.Member).ThenInclude(m => m!.User)
            .Where(e => e.GroupId == groupId)
            .OrderByDescending(e => e.Date)
            .Select(e => new GroupIncomeDto(
                e.Id,
                e.Category.ToString(),
                e.Description,
                e.Amount,
                e.Date,
                e.RecordedAt,
                e.RecordedBy.FullName,
                e.GroupMemberId,
                e.Member != null ? e.Member.User.FullName : null))
            .ToListAsync();
    }

    public async Task<int> AddEntryAsync(int groupId, AddGroupIncomeRequest request, int currentUserId)
    {
        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive)
            ?? throw new NotFoundException();

        if (membership.Role > GroupMemberRole.Treasurer)
            throw new ForbiddenException("Only Admin or Treasurer can add income entries");

        if (request.Category == GroupIncomeCategory.Penalty && request.GroupMemberId == null)
            throw new BadRequestException("A member must be selected for a penalty.");

        if (request.Category != GroupIncomeCategory.Penalty && request.GroupMemberId != null)
            throw new BadRequestException("Member should only be set for penalties.");

        if (request.GroupMemberId.HasValue)
        {
            bool memberExists = await db.GroupMembers
                .AnyAsync(m => m.Id == request.GroupMemberId.Value && m.GroupId == groupId && m.IsActive);
            if (!memberExists)
                throw new BadRequestException("Selected member is not an active member of this group.");
        }

        var entry = new GroupIncome
        {
            GroupId          = groupId,
            Category         = request.Category,
            Description      = request.Description,
            Amount           = request.Amount,
            Date             = request.Date.ToUniversalTime(),
            RecordedByUserId = currentUserId,
            GroupMemberId    = request.GroupMemberId
        };

        db.GroupIncomes.Add(entry);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "GroupIncome added — Id {Id}, GroupId {GroupId}, Category {Category}, Amount {Amount}, by UserId {UserId}",
            entry.Id, groupId, request.Category, request.Amount, currentUserId);

        return entry.Id;
    }

    public async Task DeleteEntryAsync(int groupId, int entryId, int currentUserId)
    {
        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive)
            ?? throw new NotFoundException();

        if (membership.Role != GroupMemberRole.Admin)
            throw new ForbiddenException("Only Admin can delete income entries");

        var entry = await db.GroupIncomes
            .FirstOrDefaultAsync(e => e.Id == entryId && e.GroupId == groupId)
            ?? throw new NotFoundException();

        db.GroupIncomes.Remove(entry);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "GroupIncome {Id} deleted from GroupId {GroupId} by UserId {UserId}",
            entryId, groupId, currentUserId);
    }

    private Task<bool> IsMemberAsync(int groupId, int userId) =>
        db.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.IsActive);
}
