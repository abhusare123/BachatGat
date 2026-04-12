using BachatGat.Application.Abstractions;
using BachatGat.Application.DTOs;
using BachatGat.Application.Exceptions;
using BachatGat.Application.Interfaces;
using BachatGat.Core.Entities;
using BachatGat.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace BachatGat.Application.Services;

public class ContributionService(IAppDbContext db) : IContributionService
{
    public async Task<List<ContributionDto>> GetContributionsAsync(int groupId, string? period, int currentUserId)
    {
        if (!await IsMemberAsync(groupId, currentUserId)) throw new NotFoundException();

        var query = db.Contributions
            .Include(c => c.GroupMember).ThenInclude(m => m.User)
            .Where(c => c.GroupMember.GroupId == groupId);

        if (!string.IsNullOrEmpty(period))
            query = query.Where(c => c.Period == period);

        return await query
            .OrderByDescending(c => c.Period)
            .Select(c => new ContributionDto(
                c.Id, c.GroupMemberId, c.GroupMember.User.FullName, c.Period, c.AmountPaid, c.PaidAt))
            .ToListAsync();
    }

    public async Task RecordContributionAsync(int groupId, RecordContributionRequest request, int currentUserId)
    {
        var callerMembership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive);
        if (callerMembership == null || callerMembership.Role > GroupMemberRole.Treasurer)
            throw new ForbiddenException();

        var targetMember = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.Id == request.GroupMemberId && m.GroupId == groupId && m.IsActive)
            ?? throw new BadRequestException("Member not found in this group");

        var existing = await db.Contributions
            .FirstOrDefaultAsync(c => c.GroupMemberId == request.GroupMemberId && c.Period == request.Period);
        if (existing != null)
            throw new ConflictException($"Contribution for period {request.Period} already recorded");

        db.Contributions.Add(new Contribution
        {
            GroupMemberId = request.GroupMemberId,
            Period = request.Period,
            AmountPaid = request.AmountPaid,
            RecordedByUserId = currentUserId
        });
        await db.SaveChangesAsync();
    }

    public async Task UpdateContributionAsync(int groupId, int contributionId, UpdateContributionRequest request, int currentUserId)
    {
        var callerMembership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive);
        if (callerMembership == null || callerMembership.Role > GroupMemberRole.Treasurer)
            throw new ForbiddenException();

        var contribution = await db.Contributions
            .Include(c => c.GroupMember)
            .FirstOrDefaultAsync(c => c.Id == contributionId && c.GroupMember.GroupId == groupId)
            ?? throw new NotFoundException();

        contribution.AmountPaid = request.AmountPaid;
        await db.SaveChangesAsync();
    }

    public async Task<ContributionTrackerDto> GetTrackerAsync(int groupId, int currentUserId)
    {
        if (!await IsMemberAsync(groupId, currentUserId)) throw new NotFoundException();

        var members = await db.GroupMembers
            .Include(m => m.User)
            .Include(m => m.Contributions)
            .Where(m => m.GroupId == groupId && m.IsActive)
            .OrderBy(m => m.User.FullName)
            .ToListAsync();

        var allPeriods = members
            .SelectMany(m => m.Contributions.Select(c => c.Period))
            .Distinct()
            .OrderBy(p => p)
            .ToList();

        var rows = members.Select(member =>
        {
            decimal cumulative = 0;
            var cells = allPeriods.Select(period =>
            {
                var contrib = member.Contributions.FirstOrDefault(c => c.Period == period);
                decimal paid = contrib?.AmountPaid ?? 0;
                cumulative += paid;
                return new ContributionCell(contrib?.Id, period, paid, cumulative, contrib != null);
            }).ToList();

            return new MemberTrackerRow(member.Id, member.User.FullName, cells, cumulative);
        }).ToList();

        var group = await db.Groups.FindAsync(groupId);
        var periodTotals = allPeriods.Select(period =>
        {
            decimal total = members.Sum(m => m.Contributions.FirstOrDefault(c => c.Period == period)?.AmountPaid ?? 0);
            decimal expected = group!.MonthlyAmount * members.Count;
            return new PeriodTotal(period, total, expected - total);
        }).ToList();

        decimal grandTotal = members.Sum(m => m.Contributions.Sum(c => c.AmountPaid));

        return new ContributionTrackerDto(allPeriods, rows, periodTotals, grandTotal);
    }

    private Task<bool> IsMemberAsync(int groupId, int userId) =>
        db.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.IsActive);
}
