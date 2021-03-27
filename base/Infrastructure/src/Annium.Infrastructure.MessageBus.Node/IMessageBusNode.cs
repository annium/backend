using System;
using System.Reactive;

namespace Annium.Infrastructure.MessageBus.Node
{
    public interface IMessageBusNode
    {
        IObservable<Unit> Send<T>(T data);
        IObservable<Unit> Send<T>(string topic, T data);
        IObservable<string> Listen();
        IObservable<string> Listen(string topic);
        IObservable<T> Listen<T>();
        IObservable<T> Listen<T>(string topic);
    }
}
