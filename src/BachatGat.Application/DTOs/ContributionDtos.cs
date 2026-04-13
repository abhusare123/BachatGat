using System.ComponentModel.DataAnnotations;

namespace BachatGat.Application.DTOs;

public record RecordContributionRequest(
    [Range(1, int.MaxValue, ErrorMessage = "GroupMemberId must be a valid member ID")]
    int GroupMemberId,

    [Required, RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "Period must be in YYYY-MM format")]
    string Period,

    [Range(0.01, double.MaxValue, ErrorMessage = "AmountPaid must be greater than 0")]
    decimal AmountPaid);

public record UpdateContributionRequest(
    [Range(0.01, double.MaxValue, ErrorMessage = "AmountPaid must be greater than 0")]
    decimal AmountPaid);

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
