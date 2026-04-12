using BachatGat.Application.DTOs;

namespace BachatGat.Application.Interfaces;

public interface IContributionService
{
    Task<List<ContributionDto>> GetContributionsAsync(int groupId, string? period, int currentUserId);
    Task RecordContributionAsync(int groupId, RecordContributionRequest request, int currentUserId);
    Task UpdateContributionAsync(int groupId, int contributionId, UpdateContributionRequest request, int currentUserId);
    Task<ContributionTrackerDto> GetTrackerAsync(int groupId, int currentUserId);
}
