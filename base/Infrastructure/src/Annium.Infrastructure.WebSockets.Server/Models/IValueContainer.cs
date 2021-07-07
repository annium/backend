using System;
using System.Runtime.CompilerServices;

namespace Annium.Infrastructure.WebSockets.Server.Models
{
    public interface IValueContainer<TConfig, TValue> : IValueContainer<TValue>
    {
        void Configure(TConfig config);
    }

    public interface IValueContainer<TValue>
    {
        TValue Value { get; }
        event Action<TValue> OnChange;
        void Set(TValue value);
        TaskAwaiter<TValue> GetAwaiter();
    }
}