using System.ComponentModel.DataAnnotations;
using BachatGat.Core.Enums;

namespace BachatGat.Application.DTOs;

public record AddGroupIncomeRequest(
    [EnumDataType(typeof(GroupIncomeCategory))]
    GroupIncomeCategory Category,

    [Required, MaxLength(500)]
    string Description,

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    decimal Amount,

    DateTime Date,

    int? GroupMemberId);   // required when Category == Penalty

public record GroupIncomeDto(
    int Id,
    string Category,
    string Description,
    decimal Amount,
    DateTime Date,
    DateTime RecordedAt,
    string RecordedByName,
    int? GroupMemberId,
    string? MemberName);
