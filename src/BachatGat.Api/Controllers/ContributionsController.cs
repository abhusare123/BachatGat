using System.Security.Claims;
using BachatGat.Application.DTOs;
using BachatGat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BachatGat.Api.Controllers;

[ApiController]
[Route("api/groups/{groupId:int}/contributions")]
[Authorize]
public class ContributionsController(IContributionService contributionService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetContributions(int groupId, [FromQuery] string? period)
        => Ok(await contributionService.GetContributionsAsync(groupId, period, CurrentUserId));

    [HttpPost]
    public async Task<IActionResult> RecordContribution(int groupId, [FromBody] RecordContributionRequest request)
    {
        await contributionService.RecordContributionAsync(groupId, request, CurrentUserId);
        return Ok(new { Message = "Contribution recorded" });
    }

    [HttpPut("{contributionId:int}")]
    public async Task<IActionResult> UpdateContribution(int groupId, int contributionId, [FromBody] UpdateContributionRequest request)
    {
        await contributionService.UpdateContributionAsync(groupId, contributionId, request, CurrentUserId);
        return Ok(new { Message = "Contribution updated" });
    }

    [HttpGet("tracker")]
    public async Task<IActionResult> GetTracker(int groupId)
        => Ok(await contributionService.GetTrackerAsync(groupId, CurrentUserId));

    [HttpPost("{contributionId:int}/approve")]
    public async Task<IActionResult> ApproveContribution(int groupId, int contributionId)
    {
        await contributionService.ApproveContributionAsync(groupId, contributionId, CurrentUserId);
        return Ok(new { Message = "Contribution approved." });
    }
}
