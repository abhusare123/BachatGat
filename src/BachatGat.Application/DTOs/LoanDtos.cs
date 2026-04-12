using BachatGat.Core.Enums;

namespace BachatGat.Application.DTOs;

public record RequestLoanRequest(decimal Amount, int TenureMonths, string? Purpose);

public record VoteLoanRequest(VoteChoice Vote, string? Comment);

public record LoanDto(
    int Id, int GroupId, int RequestedByUserId, string RequestedByName,
    decimal Amount, int TenureMonths, decimal InterestRatePercent,
    string? Purpose, LoanStatus Status, DateTime RequestedAt, DateTime? ApprovedAt,
    int ApproveVotes, int RejectVotes, int TotalEligibleVoters,
    VoteChoice? CurrentUserVote
);

public record LoanRepaymentDto(
    int Id, string Period, decimal EMIAmount, decimal PrincipalAmount,
    decimal InterestAmount, bool IsPaid, DateTime? PaidAt
);

public record LoanVoteDto(int VotedByUserId, string VotedByName, VoteChoice Vote, DateTime VotedAt, string? Comment);
