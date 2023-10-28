using NodaTime;

namespace Annium.Mesh.Client;

public class ClientConfiguration : IClientConfiguration
{
    public Duration ResponseTimeout { get; private set; } = Duration.FromMinutes(1);

    public ClientConfiguration WithResponseTimeout(uint timeout)
    {
        ResponseTimeout = Duration.FromSeconds(timeout);

        return this;
    }
}
