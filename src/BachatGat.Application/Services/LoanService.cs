using BachatGat.Application.Abstractions;
using BachatGat.Application.DTOs;
using BachatGat.Application.Exceptions;
using BachatGat.Application.Interfaces;
using BachatGat.Core.Entities;
using BachatGat.Core.Enums;
using BachatGat.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BachatGat.Application.Services;

public class LoanService(IAppDbContext db, ILoanCalculatorService calc) : ILoanService
{
    public async Task<List<LoanDto>> GetLoansAsync(int groupId, int currentUserId)
    {
        if (!await IsMemberAsync(groupId, currentUserId)) throw new NotFoundException();

        int eligibleVoters = await db.GroupMembers
            .CountAsync(m => m.GroupId == groupId && m.IsActive
                && m.Role != GroupMemberRole.Auditor && m.Role != GroupMemberRole.Treasurer);

        var loans = await db.Loans
            .Include(l => l.RequestedBy)
            .Include(l => l.Votes)
            .Where(l => l.GroupId == groupId)
            .OrderByDescending(l => l.RequestedAt)
            .ToListAsync();

        return loans.Select(l => MapLoan(l, eligibleVoters)).ToList();
    }

    public async Task<int> RequestLoanAsync(int groupId, RequestLoanRequest request, int currentUserId)
    {
        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive)
            ?? throw new NotFoundException();

        if (membership.Role == GroupMemberRole.Auditor) throw new ForbiddenException();

        var group = await db.Groups.FindAsync(groupId)
            ?? throw new NotFoundException();

        var loan = new Loan
        {
            GroupId = groupId,
            RequestedByUserId = currentUserId,
            Amount = request.Amount,
            TenureMonths = request.TenureMonths,
            InterestRatePercent = group.InterestRatePercent,
            Purpose = request.Purpose
        };
        db.Loans.Add(loan);
        await db.SaveChangesAsync();

        return loan.Id;
    }

    public async Task<LoanDto> GetLoanAsync(int id, int currentUserId)
    {
        var loan = await db.Loans
            .Include(l => l.RequestedBy)
            .Include(l => l.Votes)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NotFoundException();

        if (!await IsMemberAsync(loan.GroupId, currentUserId)) throw new NotFoundException();

        int eligibleVoters = await db.GroupMembers
            .CountAsync(m => m.GroupId == loan.GroupId && m.IsActive
                && m.Role != GroupMemberRole.Auditor && m.Role != GroupMemberRole.Treasurer);

        return MapLoan(loan, eligibleVoters);
    }

    public async Task<LoanStatus> VoteAsync(int id, VoteLoanRequest request, int currentUserId)
    {
        var loan = await db.Loans
            .Include(l => l.Votes)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NotFoundException();

        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == loan.GroupId && m.UserId == currentUserId && m.IsActive)
            ?? throw new NotFoundException();

        if (membership.Role == GroupMemberRole.Auditor || membership.Role == GroupMemberRole.Treasurer)
            throw new ForbiddenException();

        if (loan.Status != LoanStatus.Pending)
            throw new BadRequestException("Voting is only allowed on pending loans");

        if (loan.RequestedByUserId == currentUserId)
            throw new BadRequestException("Cannot vote on your own loan");

        if (loan.Votes.Any(v => v.VotedByUserId == currentUserId))
            throw new ConflictException("Already voted on this loan");

        db.LoanVotes.Add(new LoanVote
        {
            LoanId = id,
            VotedByUserId = currentUserId,
            Vote = request.Vote,
            Comment = request.Comment
        });

        int eligibleVoters = await db.GroupMembers
            .CountAsync(m => m.GroupId == loan.GroupId && m.IsActive
                && m.Role != GroupMemberRole.Auditor && m.Role != GroupMemberRole.Treasurer
                && m.UserId != loan.RequestedByUserId);

        int approves = loan.Votes.Count(v => v.Vote == VoteChoice.Approve) + (request.Vote == VoteChoice.Approve ? 1 : 0);
        int rejects  = loan.Votes.Count(v => v.Vote == VoteChoice.Reject)  + (request.Vote == VoteChoice.Reject  ? 1 : 0);
        int majority = (eligibleVoters / 2) + 1;

        if (approves >= majority)
        {
            loan.Status = LoanStatus.Approved;
            loan.ApprovedAt = DateTime.UtcNow;
        }
        else if (rejects >= majority)
        {
            loan.Status = LoanStatus.Rejected;
        }

        await db.SaveChangesAsync();
        return loan.Status;
    }

    public async Task DisburseAsync(int id, int currentUserId)
    {
        var loan = await db.Loans.FindAsync(id)
            ?? throw new NotFoundException();

        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == loan.GroupId && m.UserId == currentUserId && m.IsActive);
        if (membership == null || membership.Role > GroupMemberRole.Treasurer)
            throw new ForbiddenException();

        if (loan.Status != LoanStatus.Approved)
            throw new BadRequestException("Only approved loans can be disbursed");

        loan.Status = LoanStatus.Active;
        loan.DisbursedAt = DateTime.UtcNow;

        var now = DateTime.UtcNow;
        string startPeriod = $"{now.Year:D4}-{now.Month:D2}";

        var schedule = calc.GenerateSchedule(loan.Amount, loan.InterestRatePercent, loan.TenureMonths, startPeriod);
        foreach (var entry in schedule)
        {
            db.LoanRepayments.Add(new LoanRepayment
            {
                LoanId = loan.Id,
                Period = entry.Period,
                EMIAmount = entry.EMI,
                PrincipalAmount = entry.Principal,
                InterestAmount = entry.Interest
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task<List<LoanRepaymentDto>> GetRepaymentsAsync(int id, int currentUserId)
    {
        var loan = await db.Loans.FindAsync(id)
            ?? throw new NotFoundException();

        if (!await IsMemberAsync(loan.GroupId, currentUserId)) throw new NotFoundException();

        return await db.LoanRepayments
            .Where(r => r.LoanId == id)
            .OrderBy(r => r.Period)
            .Select(r => new LoanRepaymentDto(
                r.Id, r.Period, r.EMIAmount, r.PrincipalAmount, r.InterestAmount, r.IsPaid, r.PaidAt))
            .ToListAsync();
    }

    public async Task<LoanStatus> MarkRepaymentPaidAsync(int loanId, int repaymentId, int currentUserId)
    {
        var loan = await db.Loans.FindAsync(loanId)
            ?? throw new NotFoundException();

        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == loan.GroupId && m.UserId == currentUserId && m.IsActive);
        if (membership == null || membership.Role > GroupMemberRole.Treasurer)
            throw new ForbiddenException();

        var repayment = await db.LoanRepayments
            .FirstOrDefaultAsync(r => r.Id == repaymentId && r.LoanId == loanId)
            ?? throw new NotFoundException();

        if (repayment.IsPaid) throw new ConflictException("Already marked as paid");

        repayment.IsPaid = true;
        repayment.PaidAt = DateTime.UtcNow;
        repayment.RecordedByUserId = currentUserId;

        bool allPaid = await db.LoanRepayments
            .AllAsync(r => r.LoanId == loanId && (r.IsPaid || r.Id == repaymentId));
        if (allPaid) loan.Status = LoanStatus.Closed;

        await db.SaveChangesAsync();
        return loan.Status;
    }

    private static LoanDto MapLoan(Loan l, int eligibleVoters) => new(
        l.Id, l.GroupId, l.RequestedByUserId, l.RequestedBy.FullName,
        l.Amount, l.TenureMonths, l.InterestRatePercent, l.Purpose,
        l.Status, l.RequestedAt, l.ApprovedAt,
        l.Votes.Count(v => v.Vote == VoteChoice.Approve),
        l.Votes.Count(v => v.Vote == VoteChoice.Reject),
        eligibleVoters);

    private Task<bool> IsMemberAsync(int groupId, int userId) =>
        db.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.IsActive);
}
