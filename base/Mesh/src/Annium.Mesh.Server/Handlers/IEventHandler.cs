using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

public interface IEventHandler<TEvent>
{
    Task HandleAsync(
        IRequestContext<TEvent> request,
        CancellationToken ct
    );
}