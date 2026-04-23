using BachatGat.Core.Enums;

namespace BachatGat.Core.Entities;

public class GroupIncome
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public GroupIncomeCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public int RecordedByUserId { get; set; }
    public int? GroupMemberId { get; set; }   // set only when Category == Penalty

    public Group Group { get; set; } = null!;
    public User RecordedBy { get; set; } = null!;
    public GroupMember? Member { get; set; }
}
