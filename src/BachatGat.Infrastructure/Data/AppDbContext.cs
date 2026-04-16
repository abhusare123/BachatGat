using BachatGat.Application.Abstractions;
using BachatGat.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BachatGat.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<Contribution> Contributions => Set<Contribution>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<LoanVote> LoanVotes => Set<LoanVote>();
    public DbSet<LoanRepayment> LoanRepayments => Set<LoanRepayment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<GroupRuleConfig> GroupRuleConfigs => Set<GroupRuleConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.PhoneNumber).IsUnique();
            e.Property(u => u.PhoneNumber).HasMaxLength(15);
            e.Property(u => u.FullName).HasMaxLength(100);
        });

        modelBuilder.Entity<OtpCode>(e =>
        {
            e.HasIndex(o => new { o.PhoneNumber, o.IsUsed });
            e.Property(o => o.PhoneNumber).HasMaxLength(15);
            e.Property(o => o.Code).HasMaxLength(6);
        });

        modelBuilder.Entity<Group>(e =>
        {
            e.Property(g => g.Name).HasMaxLength(200);
            e.Property(g => g.MonthlyAmount).HasPrecision(18, 2);
            e.Property(g => g.InterestRatePercent).HasPrecision(5, 2);
            e.HasOne(g => g.CreatedBy).WithMany().HasForeignKey(g => g.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GroupMember>(e =>
        {
            e.HasIndex(m => new { m.GroupId, m.UserId }).IsUnique();
            e.HasOne(m => m.Group).WithMany(g => g.Members).HasForeignKey(m => m.GroupId);
            e.HasOne(m => m.User).WithMany(u => u.GroupMemberships).HasForeignKey(m => m.UserId);
        });

        modelBuilder.Entity<Contribution>(e =>
        {
            e.HasIndex(c => new { c.GroupMemberId, c.Period }).IsUnique();
            e.Property(c => c.Period).HasMaxLength(7);
            e.Property(c => c.AmountPaid).HasPrecision(18, 2);
            e.HasOne(c => c.GroupMember).WithMany(m => m.Contributions).HasForeignKey(c => c.GroupMemberId);
            e.HasOne(c => c.RecordedBy).WithMany().HasForeignKey(c => c.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.ApprovedBy).WithMany().HasForeignKey(c => c.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        modelBuilder.Entity<Loan>(e =>
        {
            e.Property(l => l.Amount).HasPrecision(18, 2);
            e.Property(l => l.InterestRatePercent).HasPrecision(5, 2);
            e.Property(l => l.Purpose).HasMaxLength(500);
            e.HasOne(l => l.Group).WithMany(g => g.Loans).HasForeignKey(l => l.GroupId);
            e.HasOne(l => l.RequestedBy).WithMany().HasForeignKey(l => l.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LoanVote>(e =>
        {
            e.HasIndex(v => new { v.LoanId, v.VotedByUserId }).IsUnique();
            e.HasOne(v => v.Loan).WithMany(l => l.Votes).HasForeignKey(v => v.LoanId);
            e.HasOne(v => v.VotedBy).WithMany().HasForeignKey(v => v.VotedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Expense>(e =>
        {
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasOne(x => x.Group).WithMany().HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.RecordedBy).WithMany().HasForeignKey(x => x.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasIndex(r => r.TokenHash).IsUnique();
            e.Property(r => r.TokenHash).HasMaxLength(64);
            e.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoanRepayment>(e =>
        {
            e.HasIndex(r => new { r.LoanId, r.Period }).IsUnique();
            e.Property(r => r.Period).HasMaxLength(7);
            e.Property(r => r.EMIAmount).HasPrecision(18, 2);
            e.Property(r => r.PrincipalAmount).HasPrecision(18, 2);
            e.Property(r => r.InterestAmount).HasPrecision(18, 2);
            e.HasOne(r => r.Loan).WithMany(l => l.Repayments).HasForeignKey(r => r.LoanId);
            e.HasOne(r => r.RecordedBy).WithMany().HasForeignKey(r => r.RecordedByUserId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        });

        modelBuilder.Entity<GroupRuleConfig>(e =>
        {
            e.HasIndex(r => new { r.GroupId, r.RuleKey }).IsUnique();
            e.Property(r => r.RuleKey).HasMaxLength(100);
            e.Property(r => r.Value).HasMaxLength(500);
            e.HasOne(r => r.Group).WithMany().HasForeignKey(r => r.GroupId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
