namespace BudgetBuddy.Infrastructure;

public sealed class ConsoleLog : ILog
{
    private readonly object _lock = new();

    public void Info(string message)
        => WriteLine("INFO", message, ConsoleColor.Gray);

    public void Warn(string message)
        => WriteLine("WARN", message, ConsoleColor.Yellow);

    public void Error(string message)
        => WriteLine("ERR ", message, ConsoleColor.Red);

    private void WriteLine(string level, string message, ConsoleColor color)
    {
        lock (_lock)
        {
            var original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"[{level}] {message}");
            Console.ForegroundColor = original;
        }
    }
}
