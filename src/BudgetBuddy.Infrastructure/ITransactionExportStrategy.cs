using System;
using BudgetBuddy.Domain;

namespace BudgetBuddy.Infrastructure;

public interface ITransactionExportStrategy
{
    Task ExportAsync(
        string path,
        IEnumerable<Transaction> transactions,
        CancellationToken cancellationToken);
}

