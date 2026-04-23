using BachatGat.Application.DTOs;

namespace BachatGat.Application.Interfaces;

public interface IGroupIncomeService
{
    Task<List<GroupIncomeDto>> GetEntriesAsync(int groupId, int currentUserId);
    Task<int> AddEntryAsync(int groupId, AddGroupIncomeRequest request, int currentUserId);
    Task DeleteEntryAsync(int groupId, int entryId, int currentUserId);
}
