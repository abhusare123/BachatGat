using System.Security.Claims;
using BachatGat.Application.DTOs;
using BachatGat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BachatGat.Api.Controllers;

[ApiController]
[Authorize]
public class LoansController(ILoanService loanService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("api/groups/{groupId:int}/loans")]
    public async Task<IActionResult> GetLoans(int groupId)
        => Ok(await loanService.GetLoansAsync(groupId, CurrentUserId));

    [HttpPost("api/groups/{groupId:int}/loans")]
    public async Task<IActionResult> RequestLoan(int groupId, [FromBody] RequestLoanRequest request)
    {
        var loanId = await loanService.RequestLoanAsync(groupId, request, CurrentUserId);
        return CreatedAtAction(nameof(GetLoan), new { id = loanId }, new { Id = loanId });
    }

    [HttpGet("api/loans/{id:int}")]
    public async Task<IActionResult> GetLoan(int id)
        => Ok(await loanService.GetLoanAsync(id, CurrentUserId));

    [HttpPost("api/loans/{id:int}/vote")]
    public async Task<IActionResult> Vote(int id, [FromBody] VoteLoanRequest request)
    {
        var status = await loanService.VoteAsync(id, request, CurrentUserId);
        return Ok(new { Message = "Vote recorded", Status = status });
    }

    [HttpPost("api/loans/{id:int}/disburse")]
    public async Task<IActionResult> Disburse(int id)
    {
        await loanService.DisburseAsync(id, CurrentUserId);
        return Ok(new { Message = "Loan disbursed and repayment schedule generated" });
    }

    [HttpGet("api/loans/{id:int}/repayments")]
    public async Task<IActionResult> GetRepayments(int id)
        => Ok(await loanService.GetRepaymentsAsync(id, CurrentUserId));

    [HttpPost("api/loans/{id:int}/repayments/{repaymentId:int}/pay")]
    public async Task<IActionResult> MarkRepaymentPaid(int id, int repaymentId)
    {
        var status = await loanService.MarkRepaymentPaidAsync(id, repaymentId, CurrentUserId);
        return Ok(new { Message = "Repayment recorded", LoanStatus = status });
    }
}
