using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure;

public sealed class CsvTransactionImportService : ITransactionImportService
{
    private readonly ILog _log;

    public CsvTransactionImportService(ILog log)
    {
        _log = log;
    }

public async Task<ImportSummary> ImportAsync(
    IEnumerable<string> filePaths,
    IRepository<Transaction, int> repository,
    CancellationToken cancellationToken)
{
    if (filePaths is null) throw new ArgumentNullException(nameof(filePaths));

    int imported = 0;
    int duplicates = 0;
    int malformed = 0;

    var fileList = filePaths.ToList();
    if (fileList.Count == 0)
    {
        _log.Warn("No files provided for import.");
        return new ImportSummary(0, 0, 0);
    }

    await Parallel.ForEachAsync(fileList, cancellationToken, async (path, ct) =>
    {
        if (!File.Exists(path))
        {
            _log.Warn($"File not found: {path}");
            return;
        }

        _log.Info($"Importing file: {path}");

        string[] lines;
        try
        {
            lines = await File.ReadAllLinesAsync(path, ct);
        }
        catch (OperationCanceledException)
        {
            _log.Warn($"Import cancelled while reading: {path}");
            return;
        }

        if (lines.Length == 0)
        {
            _log.Warn($"File is empty: {path}");
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            ct.ThrowIfCancellationRequested();

            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var cells = SplitCsvLine(line);
            var result = TransactionFactory.TryCreate(cells, i + 1);

            if (!result.IsSuccess)
            {
                Interlocked.Increment(ref malformed);
                _log.Warn(result.Error ?? $"Malformed line {i + 1} in {path}.");
                continue;
            }

            var tx = result.Value!;

            if (!repository.TryAdd(tx))
            {
                Interlocked.Increment(ref duplicates);
                _log.Warn($"Duplicate Id {tx.Id} skipped (file: {path}, line {i + 1}).");
                continue;
            }

            Interlocked.Increment(ref imported);
        }
    });

    var summary = new ImportSummary(imported, duplicates, malformed);

    _log.Info($"Import finished. Imported: {summary.Imported}, Duplicates: {summary.Duplicates}, Malformed: {summary.Malformed}");

    return summary;
}


    private static string[] SplitCsvLine(string line)
        => line.Split(',', StringSplitOptions.None);
}
