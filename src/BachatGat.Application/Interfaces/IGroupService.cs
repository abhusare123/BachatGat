using BachatGat.Application.DTOs;

namespace BachatGat.Application.Interfaces;

/// <summary>
/// Application service for group management.
/// Throws <see cref="BachatGat.Application.Exceptions.NotFoundException"/> when the caller is not a member.
/// Throws <see cref="BachatGat.Application.Exceptions.ForbiddenException"/> when the caller lacks the required role.
/// Throws <see cref="BachatGat.Application.Exceptions.ConflictException"/> on duplicate membership.
/// Throws <see cref="BachatGat.Application.Exceptions.BadRequestException"/> for invalid input.
/// </summary>
public interface IGroupService
{
    Task<List<GroupDto>> GetGroupsAsync(int currentUserId);
    Task<GroupDto> CreateGroupAsync(CreateGroupRequest request, int currentUserId);
    Task<GroupDetailDto> GetGroupAsync(int groupId, int currentUserId);
    Task UpdateGroupAsync(int groupId, UpdateGroupRequest request, int currentUserId);
    Task AddMemberAsync(int groupId, AddMemberRequest request, int currentUserId);
    Task RemoveMemberAsync(int groupId, int memberId, int currentUserId);
}
