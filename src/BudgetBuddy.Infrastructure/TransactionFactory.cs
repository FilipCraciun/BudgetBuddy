using System;
using System.Globalization;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure;

public static class TransactionFactory
{
    // Expected length: 5 or 6 
    public static Result<Transaction> TryCreate(string[] cells, int lineNumberForErrors)
    {
        if (cells is null || cells.Length < 5)
            return Result<Transaction>.Fail($"Line {lineNumberForErrors}: expected at least 5 columns.");

        // Trim all fields once
        var parts = cells.Select(s => s?.Trim() ?? string.Empty).ToArray();

        // 0: Id
        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            return Result<Transaction>.Fail($"Line {lineNumberForErrors}: invalid Id '{parts[0]}'.");

        // 1: Timestamp (yyyy-MM-dd)
        var dateRes = parts[1].TryDate();
        if (!dateRes.IsSuccess)
            return Result<Transaction>.Fail($"Line {lineNumberForErrors}: {dateRes.Error}");

        // 2: Payee (required, non-empty checked by Transaction)
        var payee = parts[2];

        // 3: Amount (decimal, invariant)
        var amountRes = parts[3].TryDec();
        if (!amountRes.IsSuccess)
            return Result<Transaction>.Fail($"Line {lineNumberForErrors}: {amountRes.Error}");

        // 4: Currency (required)
        var currency = parts[4];

        // 5: Category (optional)
        string? category = parts.Length >= 6 ? parts[5] : null;

        try
        {
            var tx = new Transaction(
                id: id,
                timestamp: dateRes.Value!,    
                payee: payee,
                amount: amountRes.Value!,     
                currency: currency,
                category: string.IsNullOrWhiteSpace(category) ? null : category
            );

            return Result<Transaction>.Ok(tx);
        }
        catch (Exception ex)
        {
            
            return Result<Transaction>.Fail($"Line {lineNumberForErrors}: {ex.Message}");
        }
    }
}
