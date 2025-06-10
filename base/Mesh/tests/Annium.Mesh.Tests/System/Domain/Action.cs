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
}
