using System.ComponentModel.DataAnnotations;
using BachatGat.Core.Enums;

namespace BachatGat.Application.DTOs;

public record AddExpenseRequest(
    [Required, MaxLength(500)]
    string Description,

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    decimal Amount,

    [EnumDataType(typeof(ExpenseCategory))]
    ExpenseCategory Category,

    DateTime Date);

public record ExpenseDto(
    int Id,
    string Description,
    decimal Amount,
    string Category,
    DateTime Date,
    DateTime RecordedAt,
    string RecordedByName);
