using System.Threading.Tasks;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

public abstract class LifeCycleHandlerBase<TState>
    where TState : ConnectionStateBase
{
    public virtual Task StartAsync(TState state) => Task.CompletedTask;
    public virtual Task EndAsync(TState state) => Task.CompletedTask;
}