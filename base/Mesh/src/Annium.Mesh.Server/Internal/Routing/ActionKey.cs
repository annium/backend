namespace Annium.Mesh.Server.Internal.Routing;

internal record struct ActionKey(ushort Version, int Action, string ActionName)
{
    public override string ToString() => $"v{Version} {ActionName} ({Action})";
}