using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Handlers;

public abstract class LifeCycleHandlerBase<TState>
    where TState : ConnectionStateBase
{
    public virtual Task StartAsync(TState state) => Task.CompletedTask;
    public virtual Task EndAsync(TState state) => Task.CompletedTask;
}