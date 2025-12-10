using System;
using System.Globalization;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure;

public sealed class CsvTransactionExportStrategy : ITransactionExportStrategy
{
    private readonly ILog _log;

    public CsvTransactionExportStrategy(ILog log)
    {
        _log = log;
    }

    public async Task ExportAsync(
        string path,
        IEnumerable<Transaction> transactions,
        CancellationToken cancellationToken)
    {
        var list = transactions.ToList();

        _log.Info($"Exporting {list.Count} transactions to CSV: {path}");

        await using var writer = new StreamWriter(
            path,
            append: false,
            encoding: System.Text.Encoding.UTF8);

        // Header
        await writer.WriteLineAsync("Id,Timestamp,Payee,Amount,Currency,Category");

        foreach (var t in list)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = string.Join(',',
                t.Id.ToString(CultureInfo.InvariantCulture),
                t.Timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                EscapeCsv(t.Payee),
                t.Amount.ToString(CultureInfo.InvariantCulture),
                t.Currency,
                EscapeCsv(t.Category));

            await writer.WriteLineAsync(line);
        }

        _log.Info("CSV export completed.");
    }

    // Tiny CSV escape: wrap in quotes if contains comma or quote, and escape quotes.
    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var needsQuotes = value.Contains(',') || value.Contains('"');

        if (!needsQuotes)
            return value;

        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}
