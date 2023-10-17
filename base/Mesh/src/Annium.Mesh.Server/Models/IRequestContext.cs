using Annium.Mesh.Server.Internal.Models;

namespace Annium.Mesh.Server.Models;

public interface IRequestContext<TRequest>
{
    TRequest Request { get; }
    ConnectionState State { get; }
    void Deconstruct(out TRequest request, out ConnectionState state);
}