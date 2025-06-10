using System;
using System.Reflection;

namespace Annium.Mesh.Server.Internal.Routing;

/// <summary>
/// Represents a route configuration for request-response message handlers.
/// </summary>
/// <param name="HandlerType">The type of the handler that processes requests.</param>
/// <param name="HandleMethod">The method that handles the requests.</param>
/// <param name="RequestType">The type of requests handled by this route.</param>
/// <param name="ResultProperty">The property that contains the result of the request processing.</param>
internal record RequestRoute(Type HandlerType, MethodInfo HandleMethod, Type RequestType, PropertyInfo ResultProperty)
{
    /// <summary>
    /// Returns a string representation of the request route.
    /// </summary>
    /// <returns>The friendly name of the handler type.</returns>
    public override string ToString() => HandlerType.FriendlyName();
}
