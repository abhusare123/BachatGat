namespace BachatGat.Core.Entities;

public class GroupRuleConfig
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string RuleKey { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int UpdatedByUserId { get; set; }

    public Group Group { get; set; } = null!;
}
