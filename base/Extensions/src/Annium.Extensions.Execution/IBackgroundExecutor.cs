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
    Task ExecuteAsync(Action task);
    Task<T> ExecuteAsync<T>(Func<T> task);
    Task ExecuteAsync(Func<ValueTask> task);
    Task<T> ExecuteAsync<T>(Func<ValueTask<T>> task);
    Task TryExecuteAsync(Action task);
    Task<T> TryExecuteAsync<T>(Func<T> task);
    Task TryExecuteAsync(Func<ValueTask> task);
    Task<T> TryExecuteAsync<T>(Func<ValueTask<T>> task);
    void Start(CancellationToken ct = default);
}