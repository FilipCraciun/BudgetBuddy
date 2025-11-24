using System;
using System.Collections.Concurrent;
using System.Reflection.Metadata;

namespace BudgetBuddy.Infrastructure;

public sealed class InMemoryRepository<T, TKey> : IRepository<T, TKey>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, T> _store = new();
    private readonly Func<T, TKey> _keySelector;

    public InMemoryRepository(Func<T, TKey> keySelector)
        => _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));

    public int Count => _store.Count;

    public bool TryAdd(T entity)
    {
        var key = _keySelector(entity);
        return _store.TryAdd(key, entity);
    }

    public bool TryGet(TKey id, out T? entity)
    {
        var ok = _store.TryGetValue(id, out var val);
        entity = ok ? val : default;
        return ok;
    }

    public bool Remove(TKey id) => _store.TryRemove(id, out _);

    public IEnumerable<T> All() => _store.Values.ToArray();
}

