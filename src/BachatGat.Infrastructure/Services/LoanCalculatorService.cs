using BachatGat.Core.Enums;
using BachatGat.Core.Interfaces;

namespace BachatGat.Infrastructure.Services;

public class LoanCalculatorService : ILoanCalculatorService
{
    public decimal CalculateEmi(decimal principal, decimal monthlyRatePercent, int tenureMonths, InterestRateType rateType)
    {
        // EqualPrincipal has a varying EMI — return the first (highest) instalment
        if (rateType == InterestRateType.EqualPrincipal)
        {
            decimal monthlyPrincipal = Math.Round(principal / tenureMonths, 2);
            decimal firstInterest = Math.Round(principal * monthlyRatePercent / 100m, 2);
            return monthlyPrincipal + firstInterest;
        }

        if (monthlyRatePercent == 0 || rateType == InterestRateType.Fixed)
        {
            decimal totalInterest = rateType == InterestRateType.Fixed
                ? principal * (monthlyRatePercent / 100m) * tenureMonths
                : 0m;
            return Math.Round((principal + totalInterest) / tenureMonths, 2);
        }

        double r = (double)monthlyRatePercent / 100.0;
        double p = (double)principal;
        double n = tenureMonths;
        double emi = p * r * Math.Pow(1 + r, n) / (Math.Pow(1 + r, n) - 1);
        return Math.Round((decimal)emi, 2);
    }

    public IReadOnlyList<AmortizationEntry> GenerateSchedule(
        decimal principal, decimal monthlyRatePercent, int tenureMonths, string startPeriod, InterestRateType rateType)
    {
        var schedule = new List<AmortizationEntry>(tenureMonths);
        var (year, month) = ParsePeriod(startPeriod);

        if (rateType == InterestRateType.EqualPrincipal)
        {
            decimal monthlyPrincipal = Math.Round(principal / tenureMonths, 2);
            decimal outstanding = principal;
            decimal r = monthlyRatePercent / 100m;

            for (int i = 0; i < tenureMonths; i++)
            {
                decimal principalPart = i == tenureMonths - 1 ? outstanding : monthlyPrincipal;
                decimal interest = Math.Round(outstanding * r, 2);
                decimal emi = principalPart + interest;
                outstanding = Math.Round(outstanding - principalPart, 2);
                string period = $"{year:D4}-{month:D2}";
                schedule.Add(new AmortizationEntry(period, emi, principalPart, interest, outstanding));
                month++;
                if (month > 12) { month = 1; year++; }
            }

            return schedule;
        }

        if (rateType == InterestRateType.Fixed)
        {
            decimal monthlyInterest = Math.Round(principal * monthlyRatePercent / 100m, 2);
            decimal monthlyPrincipal = Math.Round(principal / tenureMonths, 2);
            decimal emi = monthlyPrincipal + monthlyInterest;
            decimal outstanding = principal;

            for (int i = 0; i < tenureMonths; i++)
            {
                decimal principalPart = i == tenureMonths - 1 ? outstanding : monthlyPrincipal;
                outstanding = Math.Round(outstanding - principalPart, 2);
                string period = $"{year:D4}-{month:D2}";
                schedule.Add(new AmortizationEntry(period, emi, principalPart, monthlyInterest, outstanding));
                month++;
                if (month > 12) { month = 1; year++; }
            }
        }
        else
        {
            decimal emi = CalculateEmi(principal, monthlyRatePercent, tenureMonths, InterestRateType.Reducing);
            decimal r = monthlyRatePercent / 100m;
            decimal outstanding = principal;

            for (int i = 0; i < tenureMonths; i++)
            {
                decimal interest = Math.Round(outstanding * r, 2);
                decimal principalPart = i == tenureMonths - 1
                    ? outstanding
                    : Math.Round(emi - interest, 2);
                outstanding = Math.Round(outstanding - principalPart, 2);
                string period = $"{year:D4}-{month:D2}";
                schedule.Add(new AmortizationEntry(period, emi, principalPart, interest, outstanding));
                month++;
                if (month > 12) { month = 1; year++; }
            }
        }

        return schedule;
    }

    private static (int year, int month) ParsePeriod(string period)
    {
        var parts = period.Split('-');
        return (int.Parse(parts[0]), int.Parse(parts[1]));
    }
}
