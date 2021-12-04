using System;
using System.Reactive;

namespace Annium.Infrastructure.MessageBus.Node;

public interface IMessageBusNode : IObservable<string>
{
    IObservable<Unit> Send<T>(T data) where T : notnull;
    IObservable<object> Listen();
    IObservable<T> Listen<T>();
}