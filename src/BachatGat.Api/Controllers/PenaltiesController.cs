using System.Security.Claims;
using BachatGat.Application.DTOs;
using BachatGat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BachatGat.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/groups/{groupId:int}/penalties")]
public class PenaltiesController(IPenaltyService penaltyService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>List all penalties for a group. Visible to all members.</summary>
    [HttpGet]
    public async Task<IActionResult> GetPenalties(int groupId)
        => Ok(await penaltyService.GetPenaltiesAsync(groupId, CurrentUserId));

    /// <summary>Add a penalty to a member. Admin and Treasurer only.</summary>
    [HttpPost]
    public async Task<IActionResult> AddPenalty(int groupId, [FromBody] AddPenaltyRequest request)
    {
        var id = await penaltyService.AddPenaltyAsync(groupId, request, CurrentUserId);
        return CreatedAtAction(nameof(GetPenalties), new { groupId }, new { id });
    }
}
