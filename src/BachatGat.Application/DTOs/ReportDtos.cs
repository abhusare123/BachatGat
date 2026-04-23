namespace BachatGat.Application.DTOs;

public record MonthlyReportMemberRow(
    int Serial,
    string MemberName,
    decimal TotalContributions,
    decimal MonthlyContribution,
    decimal LoanDisbursed,
    decimal MonthlyPrincipal,
    decimal MonthlyInterest,
    decimal OutstandingLoan,
    decimal MonthlyPenalty,
    decimal TotalDue
);

public record MonthlyReportDto(
    string Period,
    string GroupName,
    IList<MonthlyReportMemberRow> Members,
    decimal TotalContributions,
    decimal TotalMonthlyContributions,
    decimal TotalLoanDisbursed,
    decimal TotalMonthlyPrincipal,
    decimal TotalMonthlyInterest,
    decimal TotalOutstandingLoan,
    decimal TotalMonthlyPenalties,
    decimal GrandTotalDue
);

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
