using System;
using System.Reflection;

namespace Annium.Mesh.Server.Internal.Routing;

internal record struct RequestData(Type HandlerType, MethodInfo HandleMethod, Type RequestType, PropertyInfo ResultProperty)
{
    public override string ToString() => HandlerType.FriendlyName();
}