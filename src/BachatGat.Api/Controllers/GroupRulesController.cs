using System.Security.Claims;
using BachatGat.Application.DTOs;
using BachatGat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BachatGat.Api.Controllers;

[ApiController]
[Route("api/groups/{groupId:int}/rules")]
[Authorize]
public class GroupRulesController(IGroupRuleService groupRuleService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetRules(int groupId)
        => Ok(await groupRuleService.GetRulesAsync(groupId, CurrentUserId));

    [HttpPut("{ruleKey}")]
    public async Task<IActionResult> UpdateRule(int groupId, string ruleKey, [FromBody] UpdateGroupRuleRequest request)
    {
        await groupRuleService.UpdateRuleAsync(groupId, ruleKey, request.Value, CurrentUserId);
        return NoContent();
    }
}
