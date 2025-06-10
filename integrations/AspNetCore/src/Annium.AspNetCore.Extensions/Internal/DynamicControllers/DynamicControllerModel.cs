using System;

namespace Annium.AspNetCore.Extensions.Internal.DynamicControllers;

/// <summary>
/// Model representing a dynamic controller configuration
/// </summary>
internal class DynamicControllerModel
{
    /// <summary>
    /// Gets the controller type
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the area name for the controller
    /// </summary>
    public string? Area { get; }

    /// <summary>
    /// Gets the dynamic key for the controller
    /// </summary>
    public string? Key { get; }

    /// <summary>
    /// Gets the controller name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the route template for the controller
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// Initializes a new instance of the DynamicControllerModel class
    /// </summary>
    /// <param name="type">The controller type</param>
    /// <param name="area">The area name for the controller</param>
    /// <param name="key">The dynamic key for the controller</param>
    /// <param name="name">The controller name</param>
    /// <param name="route">The route template for the controller</param>
    public DynamicControllerModel(Type type, string? area, string? key, string name, string route)
    {
        Type = type;
        Area = area;
        Key = key;
        Name = name;
        Route = route;
    }
}
