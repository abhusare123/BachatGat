using System.Security.Claims;
using BachatGat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BachatGat.Api.Controllers;

[ApiController]
[Authorize]
public class ReportsController(IReportService reportService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("api/groups/{groupId:int}/reports/fund-summary")]
    public async Task<IActionResult> FundSummary(int groupId)
        => Ok(await reportService.GetFundSummaryAsync(groupId, CurrentUserId));

    [HttpGet("api/groups/{groupId:int}/reports/loan-ledger")]
    public async Task<IActionResult> LoanLedger(int groupId)
        => Ok(await reportService.GetLoanLedgerAsync(groupId, CurrentUserId));

    [HttpGet("api/users/me/reports/statement")]
    public async Task<IActionResult> MemberStatement([FromQuery] int groupId)
        => Ok(await reportService.GetMemberStatementAsync(groupId, CurrentUserId));

    [HttpGet("api/groups/{groupId:int}/reports/monthly")]
    public async Task<IActionResult> MonthlyReport(int groupId, [FromQuery] string? period)
        => Ok(await reportService.GetMonthlyReportAsync(groupId, CurrentUserId, period));
}
