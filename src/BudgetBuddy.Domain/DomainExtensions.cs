using System;
using System.Globalization;

namespace BudgetBuddy.Domain;

public static class DomainExtensions
{
    //Format: 1234.56 USD
    public static string ToMoney(this decimal amount, string currency)
        => string.Create(CultureInfo.InvariantCulture, $"{amount:N2} {currency.ToUpperInvariant()}");

    // Format: 2024-06
    public static string MonthKey(this DateTime dt)
        => dt.ToString("yyy-MM", CultureInfo.InvariantCulture);
    
    //Decimal parsing with invariant culture
    public static Result<decimal> TryDec(this string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return Result<decimal>.Fail("Empty decimal.");

        if (decimal.TryParse(s.Trim(), NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value))
        {
            return Result<decimal>.Ok(value);
        }

        return Result<decimal>.Fail($"Invalid decimal: '{s}'.");
    }

    //Date parsing for CSV date format: yyy-MM-dd
    public static Result<DateTime> TryDate(this string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return Result<DateTime>.Fail("Empty date.");

        if (DateTime.TryParseExact(s.Trim(), "yyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
        {
            return Result<DateTime>.Ok(dt.Date);
        }

        return Result<DateTime>.Fail($"Invalid date: '{s}'. Expected yyy-MM-dd.");
    }

    public static decimal SumAbs(this IEnumerable<decimal> source)
        => source?.Sum(x => Math.Abs(x)) ?? 0m;

    public static decimal AverageAbs(this IEnumerable<decimal> source)
    {
        if (source is null) return 0m;
        int count = 0;
        decimal sum = 0m;
        foreach (var x in source)
        {
            sum += Math.Abs(x);
            count++;
        }
        return count == 0 ? 0m : sum / count; 
    }
}
