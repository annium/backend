using System;
using System.Threading;
using Annium.Core.Primitives;

namespace Annium.Infrastructure.WebSockets.Domain.Models
{
    public abstract class ConnectionStateBase
    {
        public Guid ConnectionId { get; }
        private readonly ManualResetEventSlim _gate = new(true);

        protected ConnectionStateBase(Guid connectionId)
        {
            ConnectionId = connectionId;
        }

        public IDisposable Lock()
        {
            _gate.Wait();
            return Disposable.Create(_gate.Set);
        }
    }
}