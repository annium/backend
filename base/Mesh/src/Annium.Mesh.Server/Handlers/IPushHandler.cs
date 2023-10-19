using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

public interface IPushHandler<TAction, TMessage> : IHandlerBase<TAction>
    where TAction : struct, Enum
{
    public Task RunAsync(IPushContext<TMessage> ctx, CancellationToken ct);
}