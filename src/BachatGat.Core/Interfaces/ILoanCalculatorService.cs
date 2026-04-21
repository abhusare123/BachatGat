using BachatGat.Core.Enums;

namespace BachatGat.Core.Interfaces;

public record AmortizationEntry(string Period, decimal EMI, decimal Principal, decimal Interest, decimal OutstandingBalance);

public interface ILoanCalculatorService
{
    decimal CalculateEmi(decimal principal, decimal monthlyRatePercent, int tenureMonths, InterestRateType rateType);
    IReadOnlyList<AmortizationEntry> GenerateSchedule(decimal principal, decimal monthlyRatePercent, int tenureMonths, string startPeriod, InterestRateType rateType);
}
