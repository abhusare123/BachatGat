using System.Security.Claims;
using BachatGat.Application.DTOs;
using BachatGat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BachatGat.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/groups/{groupId:int}/expenses")]
public class ExpensesController(IExpenseService expenseService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>List all expenses for a group. Visible to all members.</summary>
    [HttpGet]
    public async Task<IActionResult> GetExpenses(int groupId)
        => Ok(await expenseService.GetExpensesAsync(groupId, CurrentUserId));

    /// <summary>Record a new expense. Admin and Treasurer only.</summary>
    [HttpPost]
    public async Task<IActionResult> AddExpense(int groupId, [FromBody] AddExpenseRequest request)
    {
        var id = await expenseService.AddExpenseAsync(groupId, request, CurrentUserId);
        return CreatedAtAction(nameof(GetExpenses), new { groupId }, new { id });
    }

    /// <summary>Delete an expense. Admin only.</summary>
    [HttpDelete("{expenseId:int}")]
    public async Task<IActionResult> DeleteExpense(int groupId, int expenseId)
    {
        await expenseService.DeleteExpenseAsync(groupId, expenseId, CurrentUserId);
        return NoContent();
    }
}
