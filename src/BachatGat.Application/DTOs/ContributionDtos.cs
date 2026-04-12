namespace BachatGat.Application.DTOs;

public record RecordContributionRequest(int GroupMemberId, string Period, decimal AmountPaid);

public record UpdateContributionRequest(decimal AmountPaid);

public record ContributionDto(int Id, int GroupMemberId, string MemberName, string Period, decimal AmountPaid, DateTime PaidAt, bool IsApproved, DateTime? ApprovedAt);

public record ContributionTrackerDto(
    IEnumerable<string> Periods,
    IEnumerable<MemberTrackerRow> Rows,
    IEnumerable<PeriodTotal> PeriodTotals,
    decimal GrandTotal
);

public record MemberTrackerRow(
    int GroupMemberId,
    string MemberName,
    IEnumerable<ContributionCell> Cells,
    decimal RunningTotal,
    decimal NextEmi,
    decimal NextEmiSaving,
    decimal NextEmiLoanPrincipal,
    decimal NextEmiLoanInterest
);

public record ContributionCell(int? ContributionId, string Period, decimal AmountPaid, decimal CumulativeTotal, bool IsPaid, bool IsApproved);

public record PeriodTotal(string Period, decimal Total, decimal Outstanding);
