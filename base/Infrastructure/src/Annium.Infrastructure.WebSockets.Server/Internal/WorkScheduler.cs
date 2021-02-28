using System;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class WorkScheduler : IAsyncDisposable
    {
        private readonly IAsyncDisposableBox _disposable = Disposable.AsyncBox();

        public void Add(Func<Task> work)
        {
            var task = Task.Run(work);
            _disposable.Add(() => task);
        }

        public async ValueTask DisposeAsync()
        {
            await _disposable.DisposeAsync();
        }
    }
}