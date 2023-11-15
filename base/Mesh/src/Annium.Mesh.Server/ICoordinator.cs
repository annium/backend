using System;
using System.Threading.Tasks;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server;

public interface ICoordinator : IDisposable
{
    Task HandleAsync(IServerConnection connection);
}
