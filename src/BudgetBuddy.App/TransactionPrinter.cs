using System;
using BudgetBuddy.Domain;

namespace BudgetBuddy.App;

public static class TransactionPrinter
{
    public static void PrintMany(IEnumerable<Transaction> transactions)
    {
        var list = transactions
            .OrderBy(t => t.Timestamp)
            .ThenBy(t => t.Id)
            .ToList();

        if (list.Count == 0)
        {
            Console.WriteLine("No transactions found.");
            return;
        }

        Console.WriteLine("Id   Date        Payee                Amount          Category");
        Console.WriteLine("---- ---------- -------------------- --------------- ----------------");

        foreach (var t in list)
        {
            var date = t.Timestamp.ToString("yyyy-MM-dd");
            var payee = Truncate(t.Payee, 20);
            var amount = t.Amount.ToMoney(t.Currency).PadLeft(15);
            var category = Truncate(t.Category, 16);

            Console.WriteLine(
                $"{t.Id,4} {date,-10} {payee,-20} {amount} {category,-16}");
        }
    }

    private static string Truncate(string value, int max)
    {
        if (value.Length <= max) return value;
        return value.Substring(0, max - 1) + "…";
    }
}
