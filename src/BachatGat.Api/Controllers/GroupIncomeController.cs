using System.Security.Claims;
using BachatGat.Application.DTOs;
using BachatGat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BachatGat.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/groups/{groupId:int}/income")]
public class GroupIncomeController(IGroupIncomeService groupIncomeService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetEntries(int groupId)
        => Ok(await groupIncomeService.GetEntriesAsync(groupId, CurrentUserId));

    [HttpPost]
    public async Task<IActionResult> AddEntry(int groupId, [FromBody] AddGroupIncomeRequest request)
    {
        var id = await groupIncomeService.AddEntryAsync(groupId, request, CurrentUserId);
        return CreatedAtAction(nameof(GetEntries), new { groupId }, new { id });
    }

    [HttpDelete("{entryId:int}")]
    public async Task<IActionResult> DeleteEntry(int groupId, int entryId)
    {
        await groupIncomeService.DeleteEntryAsync(groupId, entryId, CurrentUserId);
        return NoContent();
    }
}
