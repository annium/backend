using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets.Internal
{
    internal class KeepAliveMonitorStub : IKeepAliveMonitor
    {
        public CancellationToken Token => CancellationToken.None;

        public void Resume()
        {
        }

        public Task PauseAsync()
        {
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync() => new(Task.CompletedTask);
    }
}