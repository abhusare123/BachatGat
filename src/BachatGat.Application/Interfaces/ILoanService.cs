using BachatGat.Application.DTOs;
using BachatGat.Core.Enums;

namespace BachatGat.Application.Interfaces;

public interface ILoanService
{
    Task<List<LoanDto>> GetLoansAsync(int groupId, int currentUserId);
    Task<int> RequestLoanAsync(int groupId, RequestLoanRequest request, int currentUserId);
    Task<LoanDto> GetLoanAsync(int id, int currentUserId);
    Task<LoanStatus> VoteAsync(int id, VoteLoanRequest request, int currentUserId);
    Task DisburseAsync(int id, int currentUserId);
    Task<List<LoanRepaymentDto>> GetRepaymentsAsync(int id, int currentUserId);
    Task<LoanStatus> MarkRepaymentPaidAsync(int loanId, int repaymentId, int currentUserId);
}
