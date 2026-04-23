namespace BachatGat.Application.DTOs;

public record FundSummaryDto(
    decimal TotalContributionsCollected,
    decimal TotalLoansDisbursed,
    decimal TotalLoanOutstanding,
    decimal TotalInterestCollected,
    decimal TotalExpenses,
    decimal TotalOtherIncome,
    decimal AvailableBalance
);

public record LoanLedgerItemDto(
    int LoanId, string MemberName, decimal OriginalAmount,
    decimal OutstandingBalance, decimal TotalInterestPaid,
    string Status, DateTime RequestedAt
);

public record MemberStatementDto(
    string MemberName, string PhoneNumber,
    IEnumerable<ContributionDto> Contributions,
    decimal TotalContributed,
    IEnumerable<LoanDto> Loans
);
