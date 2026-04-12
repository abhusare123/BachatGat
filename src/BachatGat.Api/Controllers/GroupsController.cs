using System.Security.Claims;
using BachatGat.Application.DTOs;
using BachatGat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BachatGat.Api.Controllers;

[ApiController]
[Route("api/groups")]
[Authorize]
public class GroupsController(IGroupService groupService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetGroups()
        => Ok(await groupService.GetGroupsAsync(CurrentUserId));

    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        var group = await groupService.CreateGroupAsync(request, CurrentUserId);
        return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, group);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetGroup(int id)
        => Ok(await groupService.GetGroupAsync(id, CurrentUserId));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateGroup(int id, [FromBody] UpdateGroupRequest request)
    {
        await groupService.UpdateGroupAsync(id, request, CurrentUserId);
        return NoContent();
    }

    [HttpPost("{id:int}/members")]
    public async Task<IActionResult> AddMember(int id, [FromBody] AddMemberRequest request)
    {
        await groupService.AddMemberAsync(id, request, CurrentUserId);
        return Ok(new { Message = "Member added" });
    }

    [HttpDelete("{id:int}/members/{memberId:int}")]
    public async Task<IActionResult> RemoveMember(int id, int memberId)
    {
        await groupService.RemoveMemberAsync(id, memberId, CurrentUserId);
        return NoContent();
    }
}
