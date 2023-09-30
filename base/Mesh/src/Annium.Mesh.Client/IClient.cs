using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Mesh.Client;

public interface IClient : IClientBase
{
    event Func<Task> ConnectionLost;
    event Func<Task> ConnectionRestored;
    Task ConnectAsync(CancellationToken ct = default);
    Task DisconnectAsync();
}