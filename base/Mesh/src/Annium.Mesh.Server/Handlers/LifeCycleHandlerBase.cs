using System.Threading.Tasks;

namespace Annium.Mesh.Server.Handlers;

public abstract class LifeCycleHandlerBase
{
    public virtual Task StartAsync() => Task.CompletedTask;
    public virtual Task EndAsync() => Task.CompletedTask;
}