using BachatGat.Application.Abstractions;
using BachatGat.Application.DTOs;
using BachatGat.Application.Exceptions;
using BachatGat.Application.Interfaces;
using BachatGat.Core.Entities;
using BachatGat.Core.Enums;
using BachatGat.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BachatGat.Application.Services;

public class LoanService(IAppDbContext db, ILoanCalculatorService calc, ILogger<LoanService> logger) : ILoanService
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

        return loans.Select(l => MapLoan(l, eligibleVoters, currentUserId)).ToList();
    }

    public async Task<int> RequestLoanAsync(int groupId, RequestLoanRequest request, int currentUserId)
    {
        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive)
            ?? throw new NotFoundException();

        if (membership.Role == GroupMemberRole.Auditor) throw new ForbiddenException();

        var group = await db.Groups.FindAsync(groupId)
            ?? throw new NotFoundException();

        bool isAdminOrTreasurer = membership.Role == GroupMemberRole.Admin || membership.Role == GroupMemberRole.Treasurer;

        int borrowerId = currentUserId;
        if (isAdminOrTreasurer && request.BorrowerId.HasValue && request.BorrowerId.Value != currentUserId)
        {
            var borrowerMembership = await db.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == request.BorrowerId.Value && m.IsActive)
                ?? throw new BadRequestException("Selected member is not an active member of this group.");
            if (borrowerMembership.Role == GroupMemberRole.Auditor)
                throw new BadRequestException("Auditors cannot be assigned a loan.");
            borrowerId = request.BorrowerId.Value;
        }

        var loan = new Loan
        {
            GroupId = groupId,
            RequestedByUserId = borrowerId,
            Amount = request.Amount,
            TenureMonths = request.TenureMonths,
            InterestRatePercent = group.InterestRatePercent,
            Purpose = request.Purpose,
            RequestedAt = isAdminOrTreasurer && request.LoanDate.HasValue
                ? DateTime.SpecifyKind(request.LoanDate.Value, DateTimeKind.Utc)
                : DateTime.UtcNow
        };
        db.Loans.Add(loan);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Loan requested — LoanId {LoanId}, GroupId {GroupId}, UserId {UserId}, Amount {Amount:C}, Tenure {TenureMonths} months",
            loan.Id, groupId, currentUserId, request.Amount, request.TenureMonths);

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

        return MapLoan(loan, eligibleVoters, currentUserId);
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

        var existingVote = loan.Votes.FirstOrDefault(v => v.VotedByUserId == currentUserId);
        bool isNew = existingVote == null;

        if (existingVote != null)
        {
            existingVote.Vote = request.Vote;
            existingVote.VotedAt = DateTime.UtcNow;
            existingVote.Comment = request.Comment;
        }
        else
        {
            db.LoanVotes.Add(new LoanVote
            {
                LoanId = id,
                VotedByUserId = currentUserId,
                Vote = request.Vote,
                Comment = request.Comment
            });
        }

        await db.SaveChangesAsync();

        logger.LogInformation(
            "Vote {Action} on LoanId {LoanId} by UserId {UserId} — choice: {Vote}",
            isNew ? "cast" : "changed", id, currentUserId, request.Vote);

        return loan.Status;
    }

    public async Task ApproveLoanAsync(int id, int currentUserId)
    {
        var loan = await db.Loans.FindAsync(id)
            ?? throw new NotFoundException();

        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == loan.GroupId && m.UserId == currentUserId && m.IsActive);
        if (membership == null || membership.Role > GroupMemberRole.Treasurer)
            throw new ForbiddenException();

        if (loan.Status != LoanStatus.Pending)
            throw new BadRequestException("Only pending loans can be approved");

        loan.Status = LoanStatus.Approved;
        loan.ApprovedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        logger.LogInformation(
            "LoanId {LoanId} approved by UserId {UserId} (GroupId {GroupId})",
            id, currentUserId, loan.GroupId);
    }

    public async Task RejectLoanAsync(int id, int currentUserId)
    {
        var loan = await db.Loans.FindAsync(id)
            ?? throw new NotFoundException();

        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == loan.GroupId && m.UserId == currentUserId && m.IsActive);
        if (membership == null || membership.Role > GroupMemberRole.Treasurer)
            throw new ForbiddenException();

        if (loan.Status != LoanStatus.Pending)
            throw new BadRequestException("Only pending loans can be rejected");

        loan.Status = LoanStatus.Rejected;
        await db.SaveChangesAsync();

        logger.LogInformation(
            "LoanId {LoanId} rejected by UserId {UserId} (GroupId {GroupId})",
            id, currentUserId, loan.GroupId);
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

        string startPeriod = $"{loan.RequestedAt.Year:D4}-{loan.RequestedAt.Month:D2}";

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

        logger.LogInformation(
            "LoanId {LoanId} disbursed by UserId {UserId} — Amount {Amount:C}, {Instalments} instalments from {StartPeriod}",
            id, currentUserId, loan.Amount, schedule.Count, startPeriod);
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
                r.Id, r.Period, r.EMIAmount, r.PrincipalAmount, r.InterestAmount, r.IsPaid, r.IsForeclosed, r.PaidAt))
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
        if (allPaid) { loan.Status = LoanStatus.Closed; loan.ClosedAt = DateTime.UtcNow; }

        await db.SaveChangesAsync();

        logger.LogInformation(
            "Repayment {RepaymentId} for LoanId {LoanId} marked paid by UserId {UserId} (period {Period})",
            repaymentId, loanId, currentUserId, repayment.Period);

        if (loan.Status == LoanStatus.Closed)
            logger.LogInformation("LoanId {LoanId} fully repaid — status set to Closed", loanId);

        return loan.Status;
    }

    public async Task<ForeclosureSummaryDto> GetForeclosurePreviewAsync(int id, int currentUserId)
    {
        var loan = await db.Loans.FindAsync(id) ?? throw new NotFoundException();

        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == loan.GroupId && m.UserId == currentUserId && m.IsActive);
        if (membership == null || membership.Role > GroupMemberRole.Treasurer)
            throw new ForbiddenException();

        if (loan.Status != LoanStatus.Active)
            throw new BadRequestException("Only active loans can be foreclosed");

        return await CalculateForeclosureAsync(id);
    }

    public async Task<ForeclosureSummaryDto> CloseLoanEarlyAsync(int id, int currentUserId)
    {
        var loan = await db.Loans.FindAsync(id) ?? throw new NotFoundException();

        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == loan.GroupId && m.UserId == currentUserId && m.IsActive);
        if (membership == null || membership.Role > GroupMemberRole.Treasurer)
            throw new ForbiddenException();

        if (loan.Status != LoanStatus.Active)
            throw new BadRequestException("Only active loans can be closed early");

        var summary = await CalculateForeclosureAsync(id);

        var now = DateTime.UtcNow;
        var currentPeriod = $"{now.Year:D4}-{now.Month:D2}";

        var unpaid = await db.LoanRepayments
            .Where(r => r.LoanId == id && !r.IsPaid)
            .ToListAsync();

        foreach (var r in unpaid)
        {
            r.IsPaid = true;
            r.IsForeclosed = true;
            r.PaidAt = now;
            r.RecordedByUserId = currentUserId;
            // Future interest is waived on foreclosure — zero it out so reports reflect actual collection
            if (string.Compare(r.Period, currentPeriod, StringComparison.Ordinal) > 0)
                r.InterestAmount = 0;
        }

        loan.Status = LoanStatus.Closed;
        loan.ClosedAt = now;
        await db.SaveChangesAsync();

        logger.LogInformation(
            "LoanId {LoanId} closed early by UserId {UserId} — Principal {Principal:C}, Interest {Interest:C}, Total {Total:C}",
            id, currentUserId, summary.OutstandingPrincipal, summary.ForeclosureInterest, summary.TotalAmount);

        return summary;
    }

    private async Task<ForeclosureSummaryDto> CalculateForeclosureAsync(int loanId)
    {
        var currentPeriod = $"{DateTime.UtcNow.Year:D4}-{DateTime.UtcNow.Month:D2}";

        var unpaid = await db.LoanRepayments
            .Where(r => r.LoanId == loanId && !r.IsPaid)
            .ToListAsync();

        var principal = unpaid.Sum(r => r.PrincipalAmount);
        // Interest is charged only up to the current period; future EMI interest is waived
        var interest = unpaid
            .Where(r => string.Compare(r.Period, currentPeriod, StringComparison.Ordinal) <= 0)
            .Sum(r => r.InterestAmount);

        return new ForeclosureSummaryDto(principal, interest, principal + interest);
    }

    private static LoanDto MapLoan(Loan l, int eligibleVoters, int currentUserId) => new(
        l.Id, l.GroupId, l.RequestedByUserId, l.RequestedBy.FullName,
        l.Amount, l.TenureMonths, l.InterestRatePercent, l.Purpose,
        l.Status, l.RequestedAt, l.ApprovedAt, l.ClosedAt,
        l.Votes.Count(v => v.Vote == VoteChoice.Approve),
        l.Votes.Count(v => v.Vote == VoteChoice.Reject),
        eligibleVoters,
        l.Votes.FirstOrDefault(v => v.VotedByUserId == currentUserId)?.Vote);

    private Task<bool> IsMemberAsync(int groupId, int userId) =>
        db.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.IsActive);
}
