namespace Annium.Mesh.Server.Internal.Routing;

internal record struct ActionKey(ushort Version, int Action)
{
    public override string ToString() => $"v{Version}.{Action}";
}
