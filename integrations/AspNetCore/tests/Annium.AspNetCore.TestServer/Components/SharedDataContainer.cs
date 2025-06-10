using System.Collections.Concurrent;

namespace Annium.AspNetCore.TestServer.Components;

/// <summary>
/// Container for sharing data across test server components
/// </summary>
public class SharedDataContainer
{
    /// <summary>
    /// Gets or sets a shared string value
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets a thread-safe queue for logging messages
    /// </summary>
    public ConcurrentQueue<string> Log { get; } = new();
}
