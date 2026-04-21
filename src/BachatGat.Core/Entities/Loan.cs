using BachatGat.Core.Enums;

namespace BachatGat.Core.Entities;

public class Loan
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int RequestedByUserId { get; set; }
    public decimal Amount { get; set; }
    public int TenureMonths { get; set; }
    public decimal InterestRatePercent { get; set; }
    public InterestRateType InterestRateType { get; set; } = InterestRateType.Reducing;
    public string? Purpose { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? DisbursedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public Group Group { get; set; } = null!;
    public User RequestedBy { get; set; } = null!;
    public ICollection<LoanVote> Votes { get; set; } = [];
    public ICollection<LoanRepayment> Repayments { get; set; } = [];
}
