using System;

namespace Annium.Mesh.Server.Internal.Routing;

internal record struct RequestResponseData(Type HandlerType, Type RequestType, Type ResponseType)
{
    public override string ToString() => HandlerType.FriendlyName();
}