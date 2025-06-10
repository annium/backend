using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Annium.Mesh.Server.Internal.Routing;

/// <summary>
/// Provides a type-safe store for routing information, mapping action keys to route data.
/// </summary>
/// <typeparam name="T">The type of route data to store.</typeparam>
internal class RouteTypeStore<T>
{
    /// <summary>
    /// The internal dictionary that maps action keys to route data.
    /// </summary>
    private readonly Dictionary<ActionKey, T> _routes = new();

    /// <summary>
    /// Registers a route with the specified action key and data.
    /// </summary>
    /// <param name="actionKey">The action key that identifies the route.</param>
    /// <param name="data">The route data to associate with the action key.</param>
    /// <exception cref="InvalidOperationException">Thrown when the action key is already registered.</exception>
    public void Register(ActionKey actionKey, T data)
    {
        if (_routes.TryGetValue(actionKey, out var existing))
            throw new InvalidOperationException($"Action {actionKey} is already registered by {existing}");
        _routes.Add(actionKey, data);
    }

    /// <summary>
    /// Attempts to retrieve route data for the specified action key.
    /// </summary>
    /// <param name="actionKey">The action key to look up.</param>
    /// <param name="route">When this method returns, contains the route data if found; otherwise, the default value.</param>
    /// <returns><c>true</c> if the route was found; otherwise, <c>false</c>.</returns>
    public bool TryGet(ActionKey actionKey, [MaybeNullWhen(false)] out T route)
    {
        return _routes.TryGetValue(actionKey, out route);
    }

    /// <summary>
    /// Returns a read-only collection of all registered routes.
    /// </summary>
    /// <returns>A read-only collection containing all action key and route data pairs.</returns>
    public IReadOnlyCollection<KeyValuePair<ActionKey, T>> List()
    {
        return _routes.ToArray();
    }
}
