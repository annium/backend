using NodaTime;

namespace Annium.Mesh.Client;

public interface IClientConfiguration
{
    Duration ResponseTimeout { get; }
}
