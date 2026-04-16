using BachatGat.Application.Interfaces;
using BachatGat.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BachatGat.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IContributionService, ContributionService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGroupRuleService, GroupRuleService>();
        return services;
    }
}
