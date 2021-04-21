using System.Threading;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ServerLifetime : IServerLifetimeManager, IServerLifetime
    {
        public CancellationToken Stopping => _stoppingCts.Token;
        private readonly CancellationTokenSource _stoppingCts = new();

        public void Stop() => _stoppingCts.Cancel();
    }

    internal interface IServerLifetimeManager
    {
        CancellationToken Stopping { get; }
        void Stop();
    }

    internal interface IServerLifetime
    {
        CancellationToken Stopping { get; }
    }
}