namespace Annium.Mesh.Tests.System.Domain;

/// <summary>
/// Defines the available actions that can be performed in mesh tests.
/// </summary>
public enum Action
{
    /// <summary>
    /// Echo action that returns the input message back to the caller.
    /// </summary>
    Echo,

    /// <summary>
    /// Counter action that manages and updates counter values.
    /// </summary>
    Counter,

    /// <summary>
    /// Action whose handler returns a non-Ok operation status with errors.
    /// </summary>
    Fail,

    /// <summary>
    /// Action whose handler never completes until cancelled, used to exercise caller-side cancellation.
    /// </summary>
    Hang,

    /// <summary>
    /// Action whose handler throws, used to verify a faulting handler does not tear down the connection.
    /// </summary>
    Throw,

    /// <summary>
    /// Action handled by a no-response-data request handler, used to exercise the SendAsync path.
    /// </summary>
    Notify,
}
