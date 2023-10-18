using System;
using System.Collections.Generic;

namespace Annium.Mesh.Server.Internal.Routing;

internal class RouteTypeStore<T>
{
    private readonly Dictionary<ActionKey, T> _routes = new();

    public void Register(ActionKey actionKey, T data)
    {
        if (_routes.TryGetValue(actionKey, out var existing))
            throw new InvalidOperationException($"Action {actionKey} is already registered by {existing}");
        _routes.Add(actionKey, data);
    }
}