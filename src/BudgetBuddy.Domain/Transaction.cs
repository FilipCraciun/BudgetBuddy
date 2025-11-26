


namespace BudgetBuddy.Domain;

public sealed class Transaction
{
    public const string DefaultCategory = "Uncategorized";
    public const decimal MaxAbsAmount = 1_000_000m;

    // CSV fields 
    public int Id { get; }
    public DateTime Timestamp { get; }
    public string Payee { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string Category { get; private set; }

    public Transaction(
        int id, 
        DateTime timestamp, 
        string payee, 
        decimal amount, 
        string currency,
        string? category)
    {
        if (string.IsNullOrWhiteSpace(payee))
            throw new ArgumentException("Payee cannot be empty.", nameof(payee));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        if (Math.Abs(amount) > MaxAbsAmount)
            throw new ArgumentOutOfRangeException(nameof(amount), $"Amount must be within ±{MaxAbsAmount}.");
        
        Id = id;
        Timestamp = timestamp; 
        Payee = payee.Trim();
        Amount = amount; 
        Currency = currency.Trim().ToUpperInvariant();
        Category = string.IsNullOrWhiteSpace(category) ? DefaultCategory : category.Trim();
    }
    
    public void SetCategory(string name)
    {
        Category = string.IsNullOrWhiteSpace(name) ? DefaultCategory : name.Trim();
    }

    public bool IsIncome => Amount > 0;
    public bool IsExpense => Amount < 0;
}
