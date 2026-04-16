using BachatGat.Application.DTOs;

namespace BachatGat.Application.Interfaces;

public interface IGroupRuleService
{
    Task<GroupRulesResponseDto> GetRulesAsync(int groupId, int currentUserId);
    Task UpdateRuleAsync(int groupId, string ruleKey, string value, int currentUserId);
}
