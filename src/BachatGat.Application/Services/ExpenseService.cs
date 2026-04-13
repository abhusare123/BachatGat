using BachatGat.Application.Abstractions;
using BachatGat.Application.DTOs;
using BachatGat.Application.Exceptions;
using BachatGat.Application.Interfaces;
using BachatGat.Core.Entities;
using BachatGat.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BachatGat.Application.Services;

public class ExpenseService(IAppDbContext db, ILogger<ExpenseService> logger) : IExpenseService
{
    public async Task<List<ExpenseDto>> GetExpensesAsync(int groupId, int currentUserId)
    {
        if (!await IsMemberAsync(groupId, currentUserId)) throw new NotFoundException();

        return await db.Expenses
            .Include(e => e.RecordedBy)
            .Where(e => e.GroupId == groupId)
            .OrderByDescending(e => e.Date)
            .Select(e => new ExpenseDto(
                e.Id, e.Description, e.Amount,
                e.Category.ToString(), e.Date, e.RecordedAt,
                e.RecordedBy.FullName))
            .ToListAsync();
    }

    public async Task<int> AddExpenseAsync(int groupId, AddExpenseRequest request, int currentUserId)
    {
        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive)
            ?? throw new NotFoundException();

        // Only Admin and Treasurer can record expenses
        if (membership.Role > GroupMemberRole.Treasurer)
            throw new ForbiddenException("Only Admin or Treasurer can add expenses");

        var expense = new Expense
        {
            GroupId           = groupId,
            Description       = request.Description,
            Amount            = request.Amount,
            Category          = request.Category,
            Date              = request.Date.ToUniversalTime(),
            RecordedByUserId  = currentUserId
        };

        db.Expenses.Add(expense);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Expense added — ExpenseId {ExpenseId}, GroupId {GroupId}, Category {Category}, Amount {Amount:C}, by UserId {UserId}",
            expense.Id, groupId, request.Category, request.Amount, currentUserId);

        return expense.Id;
    }

    public async Task DeleteExpenseAsync(int groupId, int expenseId, int currentUserId)
    {
        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive)
            ?? throw new NotFoundException();

        // Only Admin can delete expenses
        if (membership.Role != GroupMemberRole.Admin)
            throw new ForbiddenException("Only Admin can delete expenses");

        var expense = await db.Expenses
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.GroupId == groupId)
            ?? throw new NotFoundException();

        db.Expenses.Remove(expense);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Expense {ExpenseId} deleted from GroupId {GroupId} by UserId {UserId}",
            expenseId, groupId, currentUserId);
    }

    private Task<bool> IsMemberAsync(int groupId, int userId) =>
        db.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.IsActive);
}
