using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure;

public interface ITransactionImportService
{
    Task<ImportSummary> ImportAsync(
        IEnumerable<string> filePaths,
        IRepository<Transaction, int> repository,
        CancellationToken cancellationToken);
}

public sealed record ImportSummary(
    int Imported,
    int Duplicates,
    int Malformed);
