using BachatGat.Core.Interfaces;

namespace BachatGat.Infrastructure.Services;

public class LoanCalculatorService : ILoanCalculatorService
{
    public decimal CalculateEmi(decimal principal, decimal monthlyRatePercent, int tenureMonths)
    {
        if (monthlyRatePercent == 0)
            return Math.Round(principal / tenureMonths, 2);

        double r = (double)monthlyRatePercent / 100.0;
        double p = (double)principal;
        double n = tenureMonths;
        double emi = p * r * Math.Pow(1 + r, n) / (Math.Pow(1 + r, n) - 1);
        return Math.Round((decimal)emi, 2);
    }

    public IReadOnlyList<AmortizationEntry> GenerateSchedule(
        decimal principal, decimal monthlyRatePercent, int tenureMonths, string startPeriod)
    {
        decimal emi = CalculateEmi(principal, monthlyRatePercent, tenureMonths);
        decimal r = monthlyRatePercent / 100m;
        decimal outstanding = principal;
        var schedule = new List<AmortizationEntry>(tenureMonths);

        var (year, month) = ParsePeriod(startPeriod);

        for (int i = 0; i < tenureMonths; i++)
        {
            decimal interest = Math.Round(outstanding * r, 2);
            decimal principalPart = i == tenureMonths - 1
                ? outstanding           // last instalment clears remainder
                : Math.Round(emi - interest, 2);

            outstanding = Math.Round(outstanding - principalPart, 2);

            string period = $"{year:D4}-{month:D2}";
            schedule.Add(new AmortizationEntry(period, emi, principalPart, interest, outstanding));

            month++;
            if (month > 12) { month = 1; year++; }
        }

        return schedule;
    }

    private static (int year, int month) ParsePeriod(string period)
    {
        var parts = period.Split('-');
        return (int.Parse(parts[0]), int.Parse(parts[1]));
    }
}
