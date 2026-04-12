using BachatGat.Core.Enums;

namespace BachatGat.Application.DTOs;

public record CreateGroupRequest(string Name, string? Description, decimal MonthlyAmount, decimal InterestRatePercent);

public record UpdateGroupRequest(string Name, string? Description, decimal MonthlyAmount, decimal InterestRatePercent);

public record AddMemberRequest(string PhoneNumber, GroupMemberRole Role, string? FullName);

public record GroupDto(int Id, string Name, string? Description, decimal MonthlyAmount, decimal InterestRatePercent, DateTime CreatedAt, int MemberCount);

public record GroupDetailDto(int Id, string Name, string? Description, decimal MonthlyAmount, decimal InterestRatePercent, DateTime CreatedAt, IEnumerable<GroupMemberDto> Members);

public record GroupMemberDto(int Id, int UserId, string FullName, string PhoneNumber, GroupMemberRole Role, DateTime JoinedAt, bool IsActive);
