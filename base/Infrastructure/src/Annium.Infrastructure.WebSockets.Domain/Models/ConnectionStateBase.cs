using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace Annium.Infrastructure.WebSockets.Domain.Models
{
    public abstract class ConnectionStateBase : IAsyncDisposable
    {
        public Guid ConnectionId { get; }

        protected AsyncDisposableBox Disposable = Core.Primitives.Disposable.AsyncBox();

        private readonly ManualResetEventSlim _gate = new(true);

        protected ConnectionStateBase(Guid connectionId)
        {
            ConnectionId = connectionId;
        }

        public IDisposable Lock()
        {
            _gate.Wait();
            return Core.Primitives.Disposable.Create(_gate.Set);
        }

        public async ValueTask DisposeAsync()
        {
            _gate.Dispose();
            await DoDisposeAsync();
        }

        protected virtual Task DoDisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}