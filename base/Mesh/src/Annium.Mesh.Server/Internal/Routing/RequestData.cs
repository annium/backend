using System;

namespace Annium.Mesh.Server.Internal.Routing;

internal record struct RequestData(Type HandlerType, Type RequestType)
{
    public override string ToString() => HandlerType.FriendlyName();
}