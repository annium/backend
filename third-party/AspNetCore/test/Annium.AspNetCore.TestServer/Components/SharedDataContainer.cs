using System.Collections.Concurrent;

namespace Annium.AspNetCore.TestServer.Components;

public class SharedDataContainer
{
    public string Value { get; set; } = string.Empty;
    public ConcurrentQueue<string> Log { get; } = new();
}