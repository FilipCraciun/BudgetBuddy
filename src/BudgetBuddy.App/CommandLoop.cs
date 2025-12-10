using System.ComponentModel;
using BudgetBuddy.Domain;
using BudgetBuddy.Infrastructure;

namespace BudgetBuddy.App;

public sealed class CommandLoop
{
    private readonly IRepository<Transaction, int> _repository;
    private readonly ITransactionImportService _importer;
    private readonly ITransactionExportStrategy _jsonExporter;
    private readonly ITransactionExportStrategy _csvExporter;
    private readonly ILog _log;
    private readonly CancellationTokenSource _cts;

    public CommandLoop(
        IRepository<Transaction, int> repository,
        ITransactionImportService importer,
        ITransactionExportStrategy jsonExporter,
        ITransactionExportStrategy csvExporter,
        ILog log,
        CancellationTokenSource cts)
    {
        _repository = repository;
        _importer = importer;
        _jsonExporter = jsonExporter;
        _csvExporter = csvExporter;
        _log = log;
        _cts = cts;
    }

    public async Task RunAsync()
    {
        PrintWelcome();

        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line is null)
            {
                break;
            }

            line = line.Trim();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var command = parts[0].ToLowerInvariant();
            var args = parts.Skip(1).ToArray();

            switch (command)
            {
                case "help":
                    PrintHelp();
                    break;

                case "exit":
                    Console.WriteLine("Goodbye!");
                    return;

                case "import":
                    await HandleImportAsync(args);
                    break;

                case "list":
                    HandleList(args);
                    break;

                case "stats":
                    HandleStats(args);
                    break;

                case "set":
                    await HandleSetAsync(args);
                    break;

                case "rename":
                    HandleRename(args);
                    break;

                case "remove":
                    HandleRemove(args);
                    break;

                case "export":
                    await HandleExportAsync(args);
                    break;
                
                case "by":
                    HandleBy(args);
                    break;

                case "over":
                    HandleOver(args);
                    break;

                case "search":
                    HandleSearch(args);
                    break;

                default:
                    Console.WriteLine($"Unknown command: '{command}'. Type 'help' for the list of commands.");
                    break;
            }

            await Task.Yield();
        }
    }

    private static void PrintWelcome()
    {
        Console.WriteLine("BudgetBuddy $$$");
        Console.WriteLine("Type 'help' to see available commands. Type 'exit' to quit.");
        Console.WriteLine();
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("  help                     - Show this help.");
        Console.WriteLine("  exit                     - Quit the application.");
        Console.WriteLine("  import <file1> [file2]   - Import transactions from CSV files. (TODO)");
        Console.WriteLine("  list all                 - List all transactions. (TODO)");
        Console.WriteLine("  list month <yyyy-MM>     - List transactions for a month. (TODO)");
        Console.WriteLine("  by category <name>       - Filter by category. (TODO)");
        Console.WriteLine("  over <amount>            - Filter by minimum amount. (TODO)");
        Console.WriteLine("  search <text>            - Search payee/category. (TODO)");
        Console.WriteLine("  set category <id> <name> - Set category of a transaction. (TODO)");
        Console.WriteLine("  rename category <old> <new> - Rename a category across transactions. (TODO)");
        Console.WriteLine("  remove <id>              - Remove a transaction. (TODO)");
        Console.WriteLine("  stats month <yyyy-MM>    - Monthly stats. (TODO)");
        Console.WriteLine("  stats yearly <yyyy>      - Yearly stats. (TODO)");
        Console.WriteLine("  export json <path>       - Export all to JSON. (TODO)");
        Console.WriteLine("  export csv <path>        - Export all to CSV. (TODO)");
        Console.WriteLine();
    }

    private async Task HandleImportAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: import <file1.csv> [file2.csv ...]");
            return;
        }

        try
        {
            var summary = await _importer.ImportAsync(args, _repository, _cts.Token);

            Console.WriteLine(
                $"Imported: {summary.Imported}, " +
                $"Duplicates: {summary.Duplicates}, " +
                $"Malformed: {summary.Malformed}");
        }
        catch (OperationCanceledException)
        {
            _log.Warn("Import cancelled.");
        }
    }

    private void HandleList(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  list all");
            Console.WriteLine("  list month <yyyy-MM>");
            return;
        }

        if (args.Length >= 1 && args[0].Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            var all = _repository.All();
            TransactionPrinter.PrintMany(all);
            return;
        }

        if (args.Length >= 2 && args[0].Equals("month", StringComparison.OrdinalIgnoreCase))
        {
            var monthKey = args[1];

            var filtered = _repository
                .All()
                .Where(t => t.Timestamp.MonthKey() == monthKey);

            TransactionPrinter.PrintMany(filtered);
            return;
        }

        Console.WriteLine("Unknown list variant. Use:");
        Console.WriteLine("  list all");
        Console.WriteLine("  list month <yyyy-MM>");
    }

    private void HandleBy(string[] args)
    {
        if (args.Length < 2 || !args[0].Equals("category", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Usage: by category <name>");
            return;
        }

        var searchTerm = string.Join(' ', args.Skip(1)); 
        var termLower = searchTerm.ToLowerInvariant();

        var result = _repository
            .All()
            .Where(t => t.Category.ToLowerInvariant().Contains(termLower));

        TransactionPrinter.PrintMany(result);
    }

    private void HandleOver(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: over <amount>");
            return;
        }

        var amountText = args[0];
        var parsed = amountText.TryDec();

        if (!parsed.IsSuccess)
        {
            Console.WriteLine($"Invalid amount: {parsed.Error}");
            return;
        }

        var threshold = parsed.Value!;

        var result = _repository
            .All()
            .Where(t => t.Amount >= threshold);

        TransactionPrinter.PrintMany(result);
    }

    private void HandleSearch(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: search <text>");
            return;
        }

        var text = string.Join(' ', args);
        var needle = text.ToLowerInvariant();

        var result = _repository
            .All()
            .Where(t =>
                t.Payee.ToLowerInvariant().Contains(needle) ||
                t.Category.ToLowerInvariant().Contains(needle));

        TransactionPrinter.PrintMany(result);
    }

    private Task HandleSetAsync(string[] args)
    {
        if (args.Length < 3 || !args[0].Equals("category", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Usage: set category <id> <name>");
            return Task.CompletedTask;
        }

        if (!int.TryParse(args[1], out var id))
        {
            Console.WriteLine($"Invalid Id: '{args[1]}'.");
            return Task.CompletedTask;
        }

        var categoryName = string.Join(' ', args.Skip(2));

        if (!_repository.TryGet(id, out var tx) || tx is null)
        {
            Console.WriteLine("404 Not Found.");
            return Task.CompletedTask;
        }

        tx.SetCategory(categoryName);
        Console.WriteLine("200 OK.");

        return Task.CompletedTask;
    }

    private void HandleRename(string[] args)
    {
        if (args.Length < 3 || !args[0].Equals("category", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Usage: rename category <old> <new>");
            return;
        }

        var oldName = args[1];
        var newName = string.Join(' ', args.Skip(2));

        var oldLower = oldName.ToLowerInvariant();

        int updated = 0;

        foreach (var tx in _repository.All())
        {
            if (tx.Category.ToLowerInvariant() == oldLower)
            {
                tx.SetCategory(newName);
                updated++;
            }
        }

        Console.WriteLine($"Renamed category '{oldName}' to '{newName}' on {updated} transaction(s).");
    }

    private void HandleRemove(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: remove <id>");
            return;
        }

        if (!int.TryParse(args[0], out var id))
        {
            Console.WriteLine($"Invalid Id: '{args[0]}'.");
            return;
        }

        if (_repository.Remove(id))
        {
            Console.WriteLine("200 OK.");
        }
        else
        {
            Console.WriteLine("404 Not Found.");
        }
    }

    private void HandleStats(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  stats month <yyyy-MM>");
            Console.WriteLine("  stats yearly <yyyy>");
            return;
        }

        var mode = args[0].ToLowerInvariant();

        switch (mode)
        {
            case "month":
                HandleStatsMonth(args[1]);
                break;

            case "yearly":
                HandleStatsYearly(args[1]);
                break;

            default:
                Console.WriteLine("Unknown stats mode. Use:");
                Console.WriteLine("  stats month <yyyy-MM>");
                Console.WriteLine("  stats yearly <yyyy>");
                break;
        }
    }

    private void HandleStatsMonth(string monthKey)
    {
        var monthTx = _repository
            .All()
            .Where(t => t.Timestamp.MonthKey() == monthKey)
            .ToList();

        if (monthTx.Count == 0)
        {
            Console.WriteLine($"No transactions found for month {monthKey}.");
            return;
        }

        var currency = monthTx[0].Currency;

        var incomeTotal = monthTx
            .Where(t => t.Amount > 0)
            .Sum(t => t.Amount);

        var expenseTotal = monthTx
            .Where(t => t.Amount < 0)
            .Sum(t => t.Amount); 

        var net = incomeTotal + expenseTotal;

        var avgAbs = monthTx
            .Select(t => t.Amount)
            .AverageAbs();

        Console.WriteLine($"Stats for month {monthKey}:");
        Console.WriteLine($"  Income total : {incomeTotal.ToMoney(currency)}");
        Console.WriteLine($"  Expense total: {expenseTotal.ToMoney(currency)}");
        Console.WriteLine($"  Net          : {net.ToMoney(currency)}");
        Console.WriteLine($"  Avg size (|amount|): {avgAbs.ToMoney(currency)}");
        Console.WriteLine();

        var topExpenseCategories = monthTx
            .Where(t => t.Amount < 0)
            .GroupBy(t => t.Category)
            .Select(g => new
            {
                Category = g.Key,
                TotalAbs = g.Sum(x => Math.Abs(x.Amount))
            })
            .OrderByDescending(x => x.TotalAbs)
            .Take(3)
            .ToList();

        Console.WriteLine("Top 3 expense categories (by absolute total):");
        if (topExpenseCategories.Count == 0)
        {
            Console.WriteLine("  (no expenses)");
        }
        else
        {
            foreach (var item in topExpenseCategories)
            {
                Console.WriteLine($"  {item.Category,-20} {item.TotalAbs.ToMoney(currency)}");
            }
        }
        Console.WriteLine();
    }

    private void HandleStatsYearly(string yearText)
    {
        if (!int.TryParse(yearText, out var year))
        {
            Console.WriteLine($"Invalid year: '{yearText}'.");
            return;
        }

        var yearTx = _repository
            .All()
            .Where(t => t.Timestamp.Year == year)
            .ToList();

        if (yearTx.Count == 0)
        {
            Console.WriteLine($"No transactions found for year {year}.");
            return;
        }

        var currency = yearTx[0].Currency;

        var byMonth = yearTx
            .GroupBy(t => t.Timestamp.MonthKey())
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var income = g.Where(t => t.Amount > 0).Sum(t => t.Amount);
                var expense = g.Where(t => t.Amount < 0).Sum(t => t.Amount);
                var net = income + expense;
                return new
                {
                    MonthKey = g.Key,
                    Income = income,
                    Expense = expense,
                    Net = net
                };
            })
            .ToList();

        Console.WriteLine($"Yearly stats for {year}:");
        Console.WriteLine("Month    Income            Expense           Net");
        Console.WriteLine("-------  ----------------  ----------------  ----------------");

        foreach (var row in byMonth)
        {
            var incomeStr = row.Income.ToMoney(currency).PadLeft(16);
            var expenseStr = row.Expense.ToMoney(currency).PadLeft(16);
            var netStr = row.Net.ToMoney(currency).PadLeft(16);

            Console.WriteLine($"{row.MonthKey,-7}  {incomeStr}  {expenseStr}  {netStr}");
        }

        Console.WriteLine();
    }

    private async Task HandleExportAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  export json <path>");
            Console.WriteLine("  export csv <path>");
            return;
        }

        var format = args[0].ToLowerInvariant();
        var path = string.Join(' ', args.Skip(1));

        if (string.IsNullOrWhiteSpace(path))
        {
            Console.WriteLine("Path cannot be empty.");
            return;
        }

        var exporter = format switch
        {
            "json" => _jsonExporter,
            "csv" => _csvExporter,
            _ => null
        };

        if (exporter is null)
        {
            Console.WriteLine("Unknown export format. Use 'json' or 'csv'.");
            return;
        }

        if (File.Exists(path))
        {
            Console.Write($"File '{path}' exists. Overwrite? (y/n): ");
            var answer = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (answer != "y" && answer != "yes")
            {
                Console.WriteLine("Export cancelled by user.");
                return;
            }
        }

        var transactions = _repository.All();

        try
        {
            await exporter.ExportAsync(path, transactions, _cts.Token);
            Console.WriteLine($"Export completed: {path}");
        }
        catch (OperationCanceledException)
        {
            _log.Warn("Export cancelled.");
            Console.WriteLine("Export cancelled.");
        }
        catch (Exception ex)
        {
            _log.Error($"Export failed: {ex.Message}");
            Console.WriteLine("Export failed. See logs for details.");
        }
    }

}
