using BachatGat.Application.DTOs;

namespace BachatGat.Application.Interfaces;

public interface IReportService
{
    Task<FundSummaryDto> GetFundSummaryAsync(int groupId, int currentUserId);
    Task<List<LoanLedgerItemDto>> GetLoanLedgerAsync(int groupId, int currentUserId);
    Task<MemberStatementDto> GetMemberStatementAsync(int groupId, int currentUserId);
    Task<MonthlyReportDto> GetMonthlyReportAsync(int groupId, int currentUserId, string? period);
}
