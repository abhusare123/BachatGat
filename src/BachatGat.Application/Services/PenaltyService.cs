using BachatGat.Application.Abstractions;
using BachatGat.Application.DTOs;
using BachatGat.Application.Exceptions;
using BachatGat.Application.Interfaces;
using BachatGat.Core.Entities;
using BachatGat.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BachatGat.Application.Services;

public class PenaltyService(IAppDbContext db, ILogger<PenaltyService> logger) : IPenaltyService
{
    public async Task<List<PenaltyDto>> GetPenaltiesAsync(int groupId, int currentUserId)
    {
        if (!await IsMemberAsync(groupId, currentUserId)) throw new NotFoundException();

        return await db.Penalties
            .Include(p => p.Member).ThenInclude(m => m.User)
            .Include(p => p.AddedBy)
            .Where(p => p.GroupId == groupId)
            .OrderByDescending(p => p.Date)
            .Select(p => new PenaltyDto(
                p.Id, p.GroupMemberId, p.Member.User.FullName,
                p.Amount, p.Purpose, p.Date, p.CreatedAt, p.AddedBy.FullName))
            .ToListAsync();
    }

    public async Task<int> AddPenaltyAsync(int groupId, AddPenaltyRequest request, int currentUserId)
    {
        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive)
            ?? throw new NotFoundException();

        if (membership.Role > GroupMemberRole.Treasurer)
            throw new ForbiddenException("Only Admin or Treasurer can add penalties");

        var targetMember = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.Id == request.MemberId && m.GroupId == groupId && m.IsActive)
            ?? throw new BadRequestException("Selected member is not an active member of this group.");

        var penalty = new Penalty
        {
            GroupId       = groupId,
            GroupMemberId = request.MemberId,
            Amount        = request.Amount,
            Purpose       = request.Purpose,
            Date          = request.Date.ToUniversalTime(),
            AddedByUserId = currentUserId
        };

        db.Penalties.Add(penalty);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Penalty added — PenaltyId {PenaltyId}, GroupId {GroupId}, MemberId {MemberId}, Amount {Amount:C}, by UserId {UserId}",
            penalty.Id, groupId, request.MemberId, request.Amount, currentUserId);

        return penalty.Id;
    }

    private Task<bool> IsMemberAsync(int groupId, int userId) =>
        db.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.IsActive);
}
