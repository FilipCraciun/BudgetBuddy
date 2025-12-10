namespace BudgetBuddy.Infrastructure;

public interface ILog
{
    void Info(string message);
    void Warn(string message);
    void Error(string message);
}
