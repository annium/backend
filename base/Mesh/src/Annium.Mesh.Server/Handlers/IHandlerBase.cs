using System;

// ReSharper disable once CheckNamespace
namespace Annium.Mesh.Server;

/// <summary>
/// Base interface for all mesh handlers providing common properties for action identification and versioning.
/// </summary>
/// <typeparam name="TAction">The enum type representing the action handled by this handler.</typeparam>
public interface IHandlerBase<TAction>
    where TAction : struct, Enum
{
    /// <summary>
    /// Gets the version of the handler API.
    /// </summary>
    static abstract ushort Version { get; }

    /// <summary>
    /// Gets the specific action handled by this handler.
    /// </summary>
    static abstract TAction Action { get; }
}
