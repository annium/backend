using System;

namespace Annium.Infrastructure.WebSockets.Domain.Models
{
    public interface IConnectionState
    {
        IDisposable Lock();
        bool Contains(string key);
        bool TryGet<T>(string key, out T value) where T : notnull;
        T Get<T>(string key) where T : notnull;
        void Set<T>(string key, T value) where T : notnull;
    }
}