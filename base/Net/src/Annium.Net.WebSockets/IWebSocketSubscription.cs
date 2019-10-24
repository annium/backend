using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets
{
    public interface IWebSocketSubscription : IDisposable
    {
        Action Subscribe(Action<string> handler);
        Action Subscribe(Action<string> handler, Func<string, bool> filter);
        Action Subscribe<T>(Action<T> handler);
        Action Subscribe<T>(Action<T> handler, Func<string, bool> filter);
        Task ListenAsync(CancellationToken token);
    }
}