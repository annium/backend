using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Annium.AspNetCore.Extensions.Internal.DynamicControllers;

/// <summary>
/// Implementation of dynamic controller model pack for configuring dynamic controllers
/// </summary>
internal class DynamicControllerModelPack : IDynamicControllerModelPack
{
    /// <summary>
    /// Gets the collection of configured dynamic controller models
    /// </summary>
    internal IReadOnlyCollection<DynamicControllerModel> Models => _models;

    /// <summary>
    /// The area name for the dynamic controllers
    /// </summary>
    private string? _area;

    /// <summary>
    /// The dynamic key for the controllers
    /// </summary>
    private string? _key;

    /// <summary>
    /// The list of configured dynamic controller models
    /// </summary>
    private readonly List<DynamicControllerModel> _models = new();

    /// <summary>
    /// Sets up the model pack with area and key configuration
    /// </summary>
    /// <param name="area">The area name for controllers</param>
    /// <param name="key">The dynamic key for controllers</param>
    /// <returns>The configured model pack</returns>
    public IDynamicControllerModelPack Setup(string? area, string? key)
    {
        _area = area;
        _key = key;

        return this;
    }

    /// <summary>
    /// Adds a dynamic controller to the model pack
    /// </summary>
    /// <typeparam name="T">The controller type</typeparam>
    /// <param name="name">The controller name</param>
    /// <param name="route">The route template</param>
    /// <returns>The configured model pack</returns>
    public IDynamicControllerModelPack Add<T>(string name, string route)
        where T : ControllerBase
    {
        _models.Add(new DynamicControllerModel(typeof(T), _area, _key, name, route));

        return this;
    }
}
