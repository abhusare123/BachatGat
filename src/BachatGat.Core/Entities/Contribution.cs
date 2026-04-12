namespace BachatGat.Core.Entities;

public class Contribution
{
    public int Id { get; set; }
    public int GroupMemberId { get; set; }
    /// <summary>Period in YYYY-MM format (e.g. "2026-01")</summary>
    public string Period { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public int RecordedByUserId { get; set; }

    public GroupMember GroupMember { get; set; } = null!;
    public User RecordedBy { get; set; } = null!;
}
