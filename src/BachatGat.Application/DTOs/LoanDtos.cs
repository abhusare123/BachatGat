using System.ComponentModel.DataAnnotations;
using BachatGat.Core.Enums;

namespace BachatGat.Application.DTOs;

public record RequestLoanRequest(
    [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    decimal Amount,

    [Range(1, 360, ErrorMessage = "TenureMonths must be between 1 and 360")]
    int TenureMonths,

    [MaxLength(500)]
    string? Purpose,

    DateTime? LoanDate,

    int? BorrowerId);

public record VoteLoanRequest(
    [EnumDataType(typeof(VoteChoice))]
    VoteChoice Vote,

    [MaxLength(500)]
    string? Comment);

public record LoanDto(
    int Id, int GroupId, int RequestedByUserId, string RequestedByName,
    decimal Amount, int TenureMonths, decimal InterestRatePercent,
    string? Purpose, LoanStatus Status, DateTime RequestedAt, DateTime? ApprovedAt, DateTime? ClosedAt,
    int ApproveVotes, int RejectVotes, int TotalEligibleVoters,
    VoteChoice? CurrentUserVote
);

public record LoanRepaymentDto(
    int Id, string Period, decimal EMIAmount, decimal PrincipalAmount,
    decimal InterestAmount, bool IsPaid, bool IsForeclosed, DateTime? PaidAt
);

public record LoanVoteDto(int VotedByUserId, string VotedByName, VoteChoice Vote, DateTime VotedAt, string? Comment);

public record ForeclosureSummaryDto(
    decimal OutstandingPrincipal,
    decimal ForeclosureInterest,
    decimal TotalAmount
);
