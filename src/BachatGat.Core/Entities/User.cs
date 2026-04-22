using BachatGat.Core.Enums;

namespace BachatGat.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FirebaseUid { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public string? PinHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<GroupMember> GroupMemberships { get; set; } = [];
}
