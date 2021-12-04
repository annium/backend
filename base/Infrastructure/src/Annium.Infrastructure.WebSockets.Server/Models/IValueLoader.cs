using System.Threading.Tasks;

namespace Annium.Infrastructure.WebSockets.Server.Models;

public interface IValueLoader<TState, TConfig, TValue>
    where TState : ConnectionStateBase
{
    ValueTask<TValue> LoadAsync(TState state, TConfig config);
}

public interface IValueLoader<TState, TValue>
    where TState : ConnectionStateBase
{
    ValueTask<TValue> LoadAsync(TState state);
}