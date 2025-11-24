namespace BudgetBuddy.Infrastructure;

public interface IRepository<T, TKey>
    where TKey : notnull
{
    int Count { get; }

    bool TryAdd(T entity);
    bool TryGet(TKey id, out T? entity);
    bool Remove(TKey id);

    IEnumerable<T> All();
}
