using BachatGat.Application.Abstractions;
using BachatGat.Core.Interfaces;
using BachatGat.Infrastructure.Data;
using BachatGat.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BachatGat.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddHttpClient<ISmsService, Msg91SmsService>();
        services.AddScoped<IFirebaseTokenValidator, FirebaseTokenValidator>();
        services.AddSingleton<ILoanCalculatorService, LoanCalculatorService>();
        services.AddSingleton<IJwtService, JwtService>();

        return services;
    }
}
