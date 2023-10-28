using System;
using System.Reflection;

namespace Annium.Mesh.Server.Internal.Routing;

internal record PushRoute(Type HandlerType, MethodInfo HandleMethod, Type MessageType)
{
    public override string ToString() => HandlerType.FriendlyName();
}
