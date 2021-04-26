using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution
{
    public interface IBackgroundExecutor : IAsyncDisposable
    {
        bool IsAvailable { get; }
        void Schedule(Action work);
        void Schedule(Func<Task> work);
        void Start(CancellationToken ct = default);
    }
}