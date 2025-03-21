using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Models;
using Annium.Mesh.Tests.System.Domain;

namespace Annium.Mesh.Tests.System.Server.Demo;

internal class CounterHandler : IPushHandler<Action, CounterMessage>
{
    public static ushort Version => 1;
    public static Action Action => Action.Counter;

    public async Task RunAsync(IPushContext<CounterMessage> ctx, CancellationToken ct)
    {
        var counter = 0;
        while (!ct.IsCancellationRequested)
        {
            ctx.Send(new CounterMessage { Value = counter++ });
            await Task.Delay(5, CancellationToken.None);
        }
    }
}
