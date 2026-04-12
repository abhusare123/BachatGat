using BachatGat.Core.Enums;

namespace BachatGat.Core.Entities;

public class GroupMember
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int UserId { get; set; }
    public GroupMemberRole Role { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public Group Group { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<Contribution> Contributions { get; set; } = [];
}
