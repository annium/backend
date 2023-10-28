using System;
using System.Reactive;

namespace Annium.Infrastructure.MessageBus.Node;

public interface IMessageBusSocket : IObservable<string>, IAsyncDisposable
{
    IObservable<Unit> Send(string message);
}
