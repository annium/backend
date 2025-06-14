using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Server;
using Annium.Mesh.Tests.System.Domain;

namespace Annium.Mesh.Tests.System.Server.Demo;

/// <summary>
/// Handler for counter push messages that continuously sends incrementing counter values to connected clients.
/// </summary>
internal class CounterHandler : IPushHandler<Action, CounterMessage>
{
    /// <summary>
    /// Gets the version of this handler.
    /// </summary>
    public static ushort Version => 1;

    /// <summary>
    /// Gets the action type this handler responds to.
    /// </summary>
    public static Action Action => Action.Counter;

    /// <summary>
    /// Runs the counter handler, continuously sending incrementing counter values to all connected clients.
    /// </summary>
    /// <param name="ctx">The push context for sending counter messages.</param>
    /// <param name="ct">The cancellation token to stop the counter.</param>
    /// <returns>A task representing the asynchronous counter operation.</returns>
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
