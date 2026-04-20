using System.ComponentModel.DataAnnotations;

namespace BachatGat.Application.DTOs;

public record AddPenaltyRequest(
    int MemberId,

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    decimal Amount,

    [Required, MaxLength(500)]
    string Purpose,

    DateTime Date);

public record PenaltyDto(
    int Id,
    int GroupMemberId,
    string MemberName,
    decimal Amount,
    string Purpose,
    DateTime Date,
    DateTime CreatedAt,
    string AddedByName);
