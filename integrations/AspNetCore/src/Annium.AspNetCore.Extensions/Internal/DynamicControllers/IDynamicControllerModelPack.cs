using Microsoft.AspNetCore.Mvc;

namespace Annium.AspNetCore.Extensions.Internal.DynamicControllers;

/// <summary>
/// Interface for configuring dynamic controller model packs
/// </summary>
public interface IDynamicControllerModelPack
{
    /// <summary>
    /// Sets up the model pack with area and key configuration
    /// </summary>
    /// <param name="area">The area name for controllers</param>
    /// <param name="key">The dynamic key for controllers</param>
    /// <returns>The configured model pack</returns>
    IDynamicControllerModelPack Setup(string? area, string? key);

    /// <summary>
    /// Adds a dynamic controller to the model pack
    /// </summary>
    /// <typeparam name="T">The controller type</typeparam>
    /// <param name="name">The controller name</param>
    /// <param name="route">The route template</param>
    /// <returns>The configured model pack</returns>
    IDynamicControllerModelPack Add<T>(string name, string route)
        where T : ControllerBase;
}
