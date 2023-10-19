using System;
using System.Reflection;

namespace Annium.Mesh.Server.Internal.Routing;

internal record RequestRoute(Type HandlerType, MethodInfo HandleMethod, Type RequestType, PropertyInfo ResultProperty)
{
    public override string ToString() => HandlerType.FriendlyName();
}