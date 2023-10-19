using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

    public bool TryGet(ActionKey actionKey, [MaybeNullWhen(false)] out T route)
    {
        return _routes.TryGetValue(actionKey, out route);
    }

    public IReadOnlyCollection<KeyValuePair<ActionKey, T>> List()
    {
        return _routes.ToArray();
    }
}