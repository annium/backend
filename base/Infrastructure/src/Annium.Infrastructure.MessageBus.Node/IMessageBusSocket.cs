using System;
using System.Reactive;

namespace Annium.Infrastructure.MessageBus.Node
{
    public interface IMessageBusSocket : IAsyncDisposable
    {
        IObservable<Unit> Send(string topic, string message);
        IObservable<string> Listen(string topic);
    }
}
