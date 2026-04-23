using BachatGat.Application.Abstractions;
using BachatGat.Application.DTOs;
using BachatGat.Application.Exceptions;
using BachatGat.Application.Interfaces;
using BachatGat.Core.Entities;
using BachatGat.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BachatGat.Application.Services;

public class ContributionService(IAppDbContext db, ILogger<ContributionService> logger) : IContributionService
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
                c.Id, c.GroupMemberId, c.GroupMember.User.FullName, c.Period, c.AmountPaid, c.PaidAt,
                c.IsApproved, c.ApprovedAt))
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

        bool autoApprove = callerMembership.Role == GroupMemberRole.Admin;

        db.Contributions.Add(new Contribution
        {
            GroupMemberId = request.GroupMemberId,
            Period = request.Period,
            AmountPaid = request.AmountPaid,
            RecordedByUserId = currentUserId,
            IsApproved = autoApprove,
            ApprovedAt = autoApprove ? DateTime.UtcNow : null,
            ApprovedByUserId = autoApprove ? currentUserId : null
        });
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Contribution recorded — GroupId {GroupId}, MemberId {MemberId}, Period {Period}, Amount {Amount:C}, AutoApproved {AutoApproved}, by UserId {UserId}",
            groupId, request.GroupMemberId, request.Period, request.AmountPaid, autoApprove, currentUserId);
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

        var oldAmount = contribution.AmountPaid;
        contribution.AmountPaid = request.AmountPaid;

        // Editing resets approval (needs re-review), unless editor is Admin
        if (callerMembership.Role == GroupMemberRole.Admin)
        {
            contribution.IsApproved = true;
            contribution.ApprovedAt = DateTime.UtcNow;
            contribution.ApprovedByUserId = currentUserId;
        }
        else
        {
            contribution.IsApproved = false;
            contribution.ApprovedAt = null;
            contribution.ApprovedByUserId = null;
        }

        await db.SaveChangesAsync();

        logger.LogInformation(
            "Contribution {ContributionId} updated — Amount {OldAmount:C} → {NewAmount:C}, ApprovalReset {ApprovalReset}, by UserId {UserId}",
            contributionId, oldAmount, request.AmountPaid, callerMembership.Role != GroupMemberRole.Admin, currentUserId);
    }

    public async Task ApproveContributionAsync(int groupId, int contributionId, int currentUserId)
    {
        var callerMembership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive);
        if (callerMembership == null || callerMembership.Role != GroupMemberRole.Admin)
            throw new ForbiddenException();

        var contribution = await db.Contributions
            .Include(c => c.GroupMember)
            .FirstOrDefaultAsync(c => c.Id == contributionId && c.GroupMember.GroupId == groupId)
            ?? throw new NotFoundException();

        contribution.IsApproved = true;
        contribution.ApprovedAt = DateTime.UtcNow;
        contribution.ApprovedByUserId = currentUserId;
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Contribution {ContributionId} approved by UserId {UserId} (GroupId {GroupId})",
            contributionId, currentUserId, groupId);
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

        var group = await db.Groups.FindAsync(groupId);

        // Load active loans with repayments for current-month EMI calculation
        var activeLoans = await db.Loans
            .Include(l => l.Repayments)
            .Where(l => l.GroupId == groupId && l.Status == LoanStatus.Active)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var currentPeriod = $"{now.Year:D4}-{now.Month:D2}";

        var rows = members.Select(member =>
        {
            decimal cumulative = 0;
            var cells = allPeriods.Select(period =>
            {
                var contrib = member.Contributions.FirstOrDefault(c => c.Period == period);
                bool isApproved = contrib?.IsApproved ?? false;
                decimal paid = contrib?.AmountPaid ?? 0;
                // Only approved amounts count toward cumulative total
                cumulative += isApproved ? paid : 0;
                return new ContributionCell(contrib?.Id, period, paid, cumulative, contrib != null, isApproved);
            }).ToList();

            // Calculate current month EMI (saving + loan repayment due this month)
            var memberLoan = activeLoans.FirstOrDefault(l => l.RequestedByUserId == member.UserId);
            var currentRepayment = memberLoan?.Repayments
                .FirstOrDefault(r => r.Period == currentPeriod);

            decimal saving = group!.MonthlyAmount;
            decimal loanPrincipal = currentRepayment?.PrincipalAmount ?? 0;
            decimal loanInterest = currentRepayment?.InterestAmount ?? 0;
            decimal nextEmi = saving + loanPrincipal + loanInterest;

            return new MemberTrackerRow(
                member.Id, member.User.FullName, cells, cumulative,
                nextEmi, saving, loanPrincipal, loanInterest);
        }).ToList();

        // Period totals: only count approved contributions
        var periodTotals = allPeriods.Select(period =>
        {
            decimal total = members.Sum(m =>
                m.Contributions
                    .Where(c => c.Period == period && c.IsApproved)
                    .Sum(c => c.AmountPaid));
            decimal expected = group!.MonthlyAmount * members.Count;
            return new PeriodTotal(period, total, expected - total);
        }).ToList();

        decimal grandTotal = members.Sum(m => m.Contributions.Where(c => c.IsApproved).Sum(c => c.AmountPaid));

        return new ContributionTrackerDto(allPeriods, rows, periodTotals, grandTotal);
    }

    private Task<bool> IsMemberAsync(int groupId, int userId) =>
        db.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.IsActive);
}
