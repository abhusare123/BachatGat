using BachatGat.Application.DTOs;

namespace BachatGat.Application.Interfaces;

public interface IExpenseService
{
    Task<List<ExpenseDto>> GetExpensesAsync(int groupId, int currentUserId);
    Task<int> AddExpenseAsync(int groupId, AddExpenseRequest request, int currentUserId);
    Task DeleteExpenseAsync(int groupId, int expenseId, int currentUserId);
}
