using BachatGat.Core.Enums;

namespace BachatGat.Core.Entities;

public class LoanVote
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public int VotedByUserId { get; set; }
    public VoteChoice Vote { get; set; }
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    public string? Comment { get; set; }

    public Loan Loan { get; set; } = null!;
    public User VotedBy { get; set; } = null!;
}
