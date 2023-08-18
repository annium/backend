using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution;

public interface IBackgroundExecutor : IAsyncDisposable
{
    bool IsAvailable { get; }
    void Schedule(Action task);
    void Schedule(Func<ValueTask> task);
    bool TrySchedule(Action task);
    bool TrySchedule(Func<ValueTask> task);
    ValueTask ExecuteAsync(Action task);
    ValueTask<T> ExecuteAsync<T>(Func<T> task);
    ValueTask ExecuteAsync(Func<ValueTask> task);
    ValueTask<T> ExecuteAsync<T>(Func<ValueTask<T>> task);
    ValueTask TryExecuteAsync(Action task);
    ValueTask<T> TryExecuteAsync<T>(Func<T> task);
    ValueTask TryExecuteAsync(Func<ValueTask> task);
    ValueTask<T> TryExecuteAsync<T>(Func<ValueTask<T>> task);
    void Start(CancellationToken ct = default);
}