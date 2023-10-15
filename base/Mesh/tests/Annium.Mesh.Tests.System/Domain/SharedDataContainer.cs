using System.Collections.Concurrent;

namespace Annium.Mesh.Tests.System.Domain;

public class SharedDataContainer
{
    public string Value { get; set; } = string.Empty;
    public ConcurrentQueue<string> Log { get; } = new();
}