using System.ComponentModel.DataAnnotations;
using BachatGat.Core.Enums;


namespace BachatGat.Application.DTOs;

public record CreateGroupRequest(
    [Required, MaxLength(200)]
    string Name,

    [MaxLength(1000)]
    string? Description,

    [Range(1, double.MaxValue, ErrorMessage = "MonthlyAmount must be greater than 0")]
    decimal MonthlyAmount,

    [Range(0, 100, ErrorMessage = "InterestRatePercent must be between 0 and 100")]
    decimal InterestRatePercent,

    [EnumDataType(typeof(InterestRateType))]
    InterestRateType InterestRateType = InterestRateType.Reducing);

public record UpdateGroupRequest(
    [Required, MaxLength(200)]
    string Name,

    [MaxLength(1000)]
    string? Description,

    [Range(1, double.MaxValue, ErrorMessage = "MonthlyAmount must be greater than 0")]
    decimal MonthlyAmount,

    [Range(0, 100, ErrorMessage = "InterestRatePercent must be between 0 and 100")]
    decimal InterestRatePercent,

    [EnumDataType(typeof(InterestRateType))]
    InterestRateType InterestRateType = InterestRateType.Reducing);

public record AddMemberRequest(
    [RegularExpression(@"^\d{10,15}$", ErrorMessage = "PhoneNumber must be 10–15 digits")]
    string? PhoneNumber,

    [EmailAddress, MaxLength(200)]
    string? Email,

    [EnumDataType(typeof(GroupMemberRole))]
    GroupMemberRole Role,

    [MaxLength(100)]
    string? FullName);

public record GroupDto(int Id, string Name, string? Description, decimal MonthlyAmount, decimal InterestRatePercent, InterestRateType InterestRateType, DateTime CreatedAt, int MemberCount);

public record GroupDetailDto(int Id, string Name, string? Description, decimal MonthlyAmount, decimal InterestRatePercent, InterestRateType InterestRateType, DateTime CreatedAt, IEnumerable<GroupMemberDto> Members);

public record GroupMemberDto(int Id, int UserId, string FullName, string? PhoneNumber, string? Email, GroupMemberRole Role, DateTime JoinedAt, bool IsActive);
