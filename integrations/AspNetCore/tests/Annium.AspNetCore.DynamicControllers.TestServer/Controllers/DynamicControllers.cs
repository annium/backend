using Microsoft.AspNetCore.Mvc;

namespace Annium.AspNetCore.DynamicControllers.TestServer.Controllers;

/// <summary>
/// Response payload echoing back the route values the dynamic-controller route convention wired up,
/// so tests can assert on the convention's effects, not just on the URL matching.
/// </summary>
public class DynamicEndpointResponse
{
    /// <summary>
    /// Gets or sets a fixed marker value identifying which controller answered the request
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resolved <c>area</c> route value, or <c>null</c> when the controller has no area
    /// </summary>
    public string? Area { get; set; }

    /// <summary>
    /// Gets or sets the resolved <c>controller</c> route value, as set by the dynamic route convention
    /// </summary>
    public string Controller { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resolved <c>dynamicKey</c> route value, as set by the dynamic route convention
    /// </summary>
    public string? Key { get; set; }
}

/// <summary>
/// Controller that opts out of conventional MVC discovery via <see cref="NonControllerAttribute" />. It is
/// reachable only because <see cref="Annium.AspNetCore.Extensions.MvcBuilderExtensions.AddDynamicControllers" />
/// explicitly adds it to the controller feature and wires a plain (area-less) route for it.
/// </summary>
[NonController]
public class PlainDynamicController : ControllerBase
{
    /// <summary>
    /// Returns a payload identifying this controller and echoing the route values set by the dynamic
    /// route convention
    /// </summary>
    /// <returns>The resolved route values for this request</returns>
    [HttpGet]
    public ActionResult<DynamicEndpointResponse> Get()
    {
        return Ok(
            new DynamicEndpointResponse
            {
                Value = "plain",
                Area = RouteData.Values.TryGetValue("area", out var area) ? area as string : null,
                Controller = RouteData.Values.TryGetValue("controller", out var controller)
                    ? controller as string ?? string.Empty
                    : string.Empty,
                Key = RouteData.Values.TryGetValue("dynamicKey", out var key) ? key as string : null,
            }
        );
    }
}

/// <summary>
/// Controller that opts out of conventional MVC discovery via <see cref="NonControllerAttribute" />. It is
/// reachable only because <see cref="Annium.AspNetCore.Extensions.MvcBuilderExtensions.AddDynamicControllers" />
/// explicitly adds it to the controller feature and wires an area-prefixed route for it.
/// </summary>
[NonController]
public class AreaDynamicController : ControllerBase
{
    /// <summary>
    /// Returns a payload identifying this controller and echoing the route values set by the dynamic
    /// route convention
    /// </summary>
    /// <returns>The resolved route values for this request</returns>
    [HttpGet]
    public ActionResult<DynamicEndpointResponse> Get()
    {
        return Ok(
            new DynamicEndpointResponse
            {
                Value = "area",
                Area = RouteData.Values.TryGetValue("area", out var area) ? area as string : null,
                Controller = RouteData.Values.TryGetValue("controller", out var controller)
                    ? controller as string ?? string.Empty
                    : string.Empty,
                Key = RouteData.Values.TryGetValue("dynamicKey", out var key) ? key as string : null,
            }
        );
    }
}
