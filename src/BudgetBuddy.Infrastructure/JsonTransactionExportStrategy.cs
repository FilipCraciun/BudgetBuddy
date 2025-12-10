using System;
using System.Text.Json;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure;

public sealed class JsonTransactionExportStrategy : ITransactionExportStrategy
{
    private readonly ILog _log;
    private readonly JsonSerializerOptions _options;

    public JsonTransactionExportStrategy(ILog log)
    {
        _log = log;
        _options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public async Task ExportAsync(
        string path,
        IEnumerable<Transaction> transactions,
        CancellationToken cancellationToken)
    {
        var list = transactions.ToList();

        _log.Info($"Exporting {list.Count} transactions to JSON: {path}");

        await using var stream = new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true);

        await JsonSerializer.SerializeAsync(stream, list, _options, cancellationToken);

        _log.Info("JSON export completed.");
    }
}
