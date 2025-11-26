using System;


namespace BudgetBuddy.Domain;

public readonly struct Result
{
    public bool IsSuccess {get;}
    public string? Error {get;}

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Ok() => new(true, null);

    public static Result Fail(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            error = "Unknown error.";
        return new(false, error);
    }

    public void Match(Action onSuccess, Action<string> onFailure)
    {
        if (IsSuccess) onSuccess();
        else onFailure(Error ?? "Unknown error.");
    }
}


public readonly struct Result <T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(true, value, null);

    public static Result<T> Fail(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            error = "Unknown error.";
        return new(false, default, error);
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error ?? "Unknown error.");

    public Result<U> Map<U>(Func<T, U> mapper)
        => IsSuccess ? Result<U>.Ok(mapper(Value!)) : Result<U>.Fail(Error ?? "Unknown error.");
        
    public Result<U> Bind<U>(Func<T, Result<U>>binder)
        => IsSuccess ? binder(Value!) : Result<U>.Fail(Error ?? "Unknown error.");
}
