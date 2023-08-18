using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution;

public interface IBackgroundExecutor : IAsyncDisposable
{
    bool IsAvailable { get; }
    void Schedule(Action task);
    void Schedule(Action<CancellationToken> task);
    void Schedule(Func<ValueTask> task);
    void Schedule(Func<CancellationToken, ValueTask> task);
    bool TrySchedule(Action task);
    bool TrySchedule(Action<CancellationToken> task);
    bool TrySchedule(Func<ValueTask> task);
    bool TrySchedule(Func<CancellationToken, ValueTask> task);
    ValueTask ExecuteAsync(Action task);
    ValueTask ExecuteAsync(Action<CancellationToken> task);
    ValueTask<T> ExecuteAsync<T>(Func<T> task);
    ValueTask<T> ExecuteAsync<T>(Func<CancellationToken, T> task);
    ValueTask ExecuteAsync(Func<ValueTask> task);
    ValueTask ExecuteAsync(Func<CancellationToken, ValueTask> task);
    ValueTask<T> ExecuteAsync<T>(Func<ValueTask<T>> task);
    ValueTask<T> ExecuteAsync<T>(Func<CancellationToken, ValueTask<T>> task);
    ValueTask TryExecuteAsync(Action task);
    ValueTask TryExecuteAsync(Action<CancellationToken> task);
    ValueTask<T> TryExecuteAsync<T>(Func<T> task);
    ValueTask<T> TryExecuteAsync<T>(Func<CancellationToken, T> task);
    ValueTask TryExecuteAsync(Func<ValueTask> task);
    ValueTask TryExecuteAsync(Func<CancellationToken, ValueTask> task);
    ValueTask<T> TryExecuteAsync<T>(Func<ValueTask<T>> task);
    ValueTask<T> TryExecuteAsync<T>(Func<CancellationToken, ValueTask<T>> task);
    void Start(CancellationToken ct = default);
}