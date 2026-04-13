using BachatGat.Application.Abstractions;
using BachatGat.Application.DTOs;
using BachatGat.Application.Exceptions;
using BachatGat.Application.Interfaces;
using BachatGat.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace BachatGat.Application.Services;

public class ReportService(IAppDbContext db) : IReportService
{
    public async Task<FundSummaryDto> GetFundSummaryAsync(int groupId, int currentUserId)
    {
        if (!await IsMemberAsync(groupId, currentUserId)) throw new NotFoundException();

        decimal totalCollected = await db.Contributions
            .Where(c => c.GroupMember.GroupId == groupId)
            .SumAsync(c => c.AmountPaid);

        var activeLoans = await db.Loans
            .Include(l => l.Repayments)
            .Where(l => l.GroupId == groupId && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Closed))
            .ToListAsync();

        decimal totalDisbursed         = activeLoans.Sum(l => l.Amount);
        decimal totalInterestCollected = activeLoans.SelectMany(l => l.Repayments.Where(r => r.IsPaid)).Sum(r => r.InterestAmount);
        decimal totalPrincipalRepaid   = activeLoans.SelectMany(l => l.Repayments.Where(r => r.IsPaid)).Sum(r => r.PrincipalAmount);
        decimal outstanding            = totalDisbursed - totalPrincipalRepaid;

        decimal totalExpenses = await db.Expenses
            .Where(e => e.GroupId == groupId)
            .SumAsync(e => e.Amount);

        decimal available = totalCollected + totalInterestCollected - totalDisbursed + totalPrincipalRepaid - totalExpenses;

        return new FundSummaryDto(totalCollected, totalDisbursed, outstanding, totalInterestCollected, totalExpenses, available);
    }

    public async Task<List<LoanLedgerItemDto>> GetLoanLedgerAsync(int groupId, int currentUserId)
    {
        if (!await IsMemberAsync(groupId, currentUserId)) throw new NotFoundException();

        var loans = await db.Loans
            .Include(l => l.RequestedBy)
            .Include(l => l.Repayments)
            .Where(l => l.GroupId == groupId && l.Status != LoanStatus.Pending && l.Status != LoanStatus.Rejected)
            .OrderByDescending(l => l.RequestedAt)
            .ToListAsync();

        return loans.Select(l =>
        {
            decimal principalRepaid = l.Repayments.Where(r => r.IsPaid).Sum(r => r.PrincipalAmount);
            decimal interestPaid    = l.Repayments.Where(r => r.IsPaid).Sum(r => r.InterestAmount);
            return new LoanLedgerItemDto(
                l.Id, l.RequestedBy.FullName, l.Amount,
                l.Amount - principalRepaid, interestPaid, l.Status.ToString(), l.RequestedAt);
        }).ToList();
    }

    public async Task<MemberStatementDto> GetMemberStatementAsync(int groupId, int currentUserId)
    {
        var membership = await db.GroupMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive)
            ?? throw new NotFoundException();

        var contributions = await db.Contributions
            .Where(c => c.GroupMemberId == membership.Id)
            .OrderBy(c => c.Period)
            .Select(c => new ContributionDto(
                c.Id, c.GroupMemberId, membership.User.FullName, c.Period, c.AmountPaid, c.PaidAt,
                c.IsApproved, c.ApprovedAt))
            .ToListAsync();

        decimal totalContributed = contributions.Sum(c => c.AmountPaid);

        int eligibleVoters = await db.GroupMembers
            .CountAsync(m => m.GroupId == groupId && m.IsActive
                && m.Role != GroupMemberRole.Auditor && m.Role != GroupMemberRole.Treasurer);

        var loans = await db.Loans
            .Include(l => l.RequestedBy)
            .Include(l => l.Votes)
            .Where(l => l.GroupId == groupId && l.RequestedByUserId == currentUserId)
            .ToListAsync();

        var loanDtos = loans.Select(l => new LoanDto(
            l.Id, l.GroupId, l.RequestedByUserId, l.RequestedBy.FullName,
            l.Amount, l.TenureMonths, l.InterestRatePercent, l.Purpose,
            l.Status, l.RequestedAt, l.ApprovedAt,
            l.Votes.Count(v => v.Vote == VoteChoice.Approve),
            l.Votes.Count(v => v.Vote == VoteChoice.Reject),
            eligibleVoters,
            CurrentUserVote: null)).ToList();

        return new MemberStatementDto(
            membership.User.FullName, membership.User.PhoneNumber,
            contributions, totalContributed, loanDtos);
    }

    private Task<bool> IsMemberAsync(int groupId, int userId) =>
        db.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.IsActive);
}
