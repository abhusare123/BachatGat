using BachatGat.Application.DTOs;

namespace BachatGat.Application.Interfaces;

public interface IPenaltyService
{
    Task<List<PenaltyDto>> GetPenaltiesAsync(int groupId, int currentUserId);
    Task<int> AddPenaltyAsync(int groupId, AddPenaltyRequest request, int currentUserId);
}
