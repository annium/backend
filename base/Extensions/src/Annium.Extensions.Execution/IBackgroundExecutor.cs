using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution
{
    public interface IBackgroundExecutor : IAsyncDisposable
    {
        bool IsAvailable { get; }
        void Schedule(Action task);
        void Schedule(Func<Task> task);
        void TrySchedule(Action task);
        void TrySchedule(Func<Task> task);
        void Start(CancellationToken ct = default);
    }
}