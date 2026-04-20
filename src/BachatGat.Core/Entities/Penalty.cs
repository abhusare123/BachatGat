namespace BachatGat.Core.Entities;

public class Penalty
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int GroupMemberId { get; set; }
    public decimal Amount { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int AddedByUserId { get; set; }

    public Group Group { get; set; } = null!;
    public GroupMember Member { get; set; } = null!;
    public User AddedBy { get; set; } = null!;
}
