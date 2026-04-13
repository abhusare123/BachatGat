using BachatGat.Core.Enums;

namespace BachatGat.Core.Entities;

public class Expense
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; }
    /// <summary>Calendar date the expense was actually incurred (not the entry date).</summary>
    public DateTime Date { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public int RecordedByUserId { get; set; }

    public Group Group { get; set; } = null!;
    public User RecordedBy { get; set; } = null!;
}
