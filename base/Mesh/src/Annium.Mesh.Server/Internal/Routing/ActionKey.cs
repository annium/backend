namespace Annium.Mesh.Server.Internal.Routing;

/// <summary>
/// Represents a key that uniquely identifies an action by its version and action identifier.
/// </summary>
/// <param name="Version">The version of the action.</param>
/// <param name="Action">The action identifier.</param>
internal record struct ActionKey(ushort Version, int Action)
{
    /// <summary>
    /// Returns a string representation of the action key.
    /// </summary>
    /// <returns>A string in the format "v{Version}.{Action}".</returns>
    public override string ToString() => $"v{Version}.{Action}";
}
