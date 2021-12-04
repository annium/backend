using System;
using System.Runtime.CompilerServices;

namespace Annium.Infrastructure.WebSockets.Server.Models;

public interface IValueContainer<TState, TConfig, TValue> : IValueContainer<TState, TValue>
    where TState : ConnectionStateBase
{
    void Configure(TConfig config);
}

public interface IValueContainer<TState, TValue> : IAsyncDisposable
    where TState : ConnectionStateBase
{
    void Bind(TState state);

    TValue Value { get; }
    event Action<TValue> OnChange;
    void Set(TValue value);
    TaskAwaiter<TValue> GetAwaiter();
}