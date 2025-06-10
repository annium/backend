using System;
using System.Reflection;

namespace Annium.Mesh.Server.Internal.Routing;

/// <summary>
/// Represents a route configuration for push message handlers.
/// </summary>
/// <param name="HandlerType">The type of the handler that processes push messages.</param>
/// <param name="HandleMethod">The method that handles the push messages.</param>
/// <param name="MessageType">The type of messages handled by this route.</param>
internal record PushRoute(Type HandlerType, MethodInfo HandleMethod, Type MessageType)
{
    /// <summary>
    /// Returns a string representation of the push route.
    /// </summary>
    /// <returns>The friendly name of the handler type.</returns>
    public override string ToString() => HandlerType.FriendlyName();
}
