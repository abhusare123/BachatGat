namespace BachatGat.Application.DTOs;

public record RecordContributionRequest(int GroupMemberId, string Period, decimal AmountPaid);

public record UpdateContributionRequest(decimal AmountPaid);

public record ContributionDto(int Id, int GroupMemberId, string MemberName, string Period, decimal AmountPaid, DateTime PaidAt);

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
    decimal RunningTotal
);

public record ContributionCell(int? ContributionId, string Period, decimal AmountPaid, decimal CumulativeTotal, bool IsPaid);

public record PeriodTotal(string Period, decimal Total, decimal Outstanding);
