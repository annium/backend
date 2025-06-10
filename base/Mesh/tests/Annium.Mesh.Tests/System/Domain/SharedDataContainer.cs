using System.Collections.Concurrent;

namespace Annium.Mesh.Tests.System.Domain;

/// <summary>
/// Provides a shared data container for storing test data that can be accessed across different components during mesh testing.
/// </summary>
public class SharedDataContainer
{
    /// <summary>
    /// Gets the concurrent log queue for storing string log messages during test execution.
    /// </summary>
    public ConcurrentQueue<string> Log { get; } = new();
}
