using System.Collections.Concurrent;

namespace Annium.Mesh.Tests.System.Domain;

public class SharedDataContainer
{
    public ConcurrentQueue<string> Log { get; } = new();
}
