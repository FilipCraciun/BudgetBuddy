using BudgetBuddy.Domain;
using BudgetBuddy.Infrastructure;

namespace BudgetBuddy.App;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var repository = new InMemoryRepository<Transaction, int>(t => t.Id);
        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            Console.WriteLine();
            Console.WriteLine("Cancellation requested...");

            cts.Cancel();
            eventArgs.Cancel = true;
        };

        ILog log = new ConsoleLog();
        ITransactionImportService importer = new CsvTransactionImportService(log);

        ITransactionExportStrategy jsonExporter = new JsonTransactionExportStrategy(log);
        ITransactionExportStrategy csvExporter = new CsvTransactionExportStrategy(log);

        var app = new CommandLoop(repository, importer, jsonExporter, csvExporter, log, cts);

        await app.RunAsync();
    }
}
