namespace BachatGat.Core.Entities;

public class LoanRepayment
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    /// <summary>Period in YYYY-MM format (e.g. "2026-01")</summary>
    public string Period { get; set; } = string.Empty;
    public decimal EMIAmount { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public int? RecordedByUserId { get; set; }

    public Loan Loan { get; set; } = null!;
    public User? RecordedBy { get; set; }
}
