using BachatGat.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BachatGat.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<OtpCode> OtpCodes { get; }
    DbSet<Group> Groups { get; }
    DbSet<GroupMember> GroupMembers { get; }
    DbSet<Contribution> Contributions { get; }
    DbSet<Loan> Loans { get; }
    DbSet<LoanVote> LoanVotes { get; }
    DbSet<LoanRepayment> LoanRepayments { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Expense> Expenses { get; }
    DbSet<GroupRuleConfig> GroupRuleConfigs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
