using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

public interface ISubscriptionHandler<TInit, TMessage>
{
    Task HandleAsync(
        ISubscriptionContext<TInit, TMessage> request,
        CancellationToken ct
    );
}