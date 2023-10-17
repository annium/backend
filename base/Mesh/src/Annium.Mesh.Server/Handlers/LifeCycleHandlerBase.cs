using System.Threading.Tasks;
using Annium.Mesh.Server.Internal.Models;

namespace Annium.Mesh.Server.Handlers;

public abstract class LifeCycleHandlerBase
{
    public virtual Task StartAsync(ConnectionState state) => Task.CompletedTask;
    public virtual Task EndAsync(ConnectionState state) => Task.CompletedTask;
}