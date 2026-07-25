using Microsoft.AspNetCore.Mvc;

namespace Annium.AspNetCore.DynamicControllers.TestServer.Controllers;

/// <summary>
/// Response payload for the conventional controller. Kept structurally identical to
/// <see cref="DynamicEndpointResponse" /> (rather than a plain string) so a test can assert that none of the
/// dynamic-controller route values (<c>area</c>/<c>controller</c>/<c>dynamicKey</c>) leaked onto this controller's
/// route data.
/// </summary>
public class ConventionalEndpointResponse
{
    /// <summary>
    /// Gets or sets a fixed marker value identifying this endpoint
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resolved <c>area</c> route value, expected to be absent for this controller
    /// </summary>
    public string? Area { get; set; }

    /// <summary>
    /// Gets or sets the resolved <c>controller</c> route value, expected to be the default MVC-assigned one
    /// rather than a value wired by the dynamic route convention
    /// </summary>
    public string? Controller { get; set; }

    /// <summary>
    /// Gets or sets the resolved <c>dynamicKey</c> route value, expected to be absent for this controller
    /// </summary>
    public string? Key { get; set; }
}

/// <summary>
/// Conventional MVC controller, discovered through the default controller-discovery mechanism (not via
/// <see cref="Annium.AspNetCore.Extensions.MvcBuilderExtensions.AddDynamicControllers" />). Registered alongside
/// the dynamic controllers in <see cref="ServicePack" /> so a request against its own fixed route exercises the
/// <c>model is null</c> guard branch of <c>DynamicControllerRouteConvention.Apply</c>: the convention runs for
/// every controller in the app, including this one, and must find no matching dynamic model and leave its
/// routing untouched.
/// </summary>
[ApiController]
[Route("conventional")]
public class ConventionalController : ControllerBase
{
    /// <summary>
    /// Returns a fixed payload from this controller's own route, echoing the route values present after model
    /// binding so tests can assert none of the dynamic-controller route values were injected
    /// </summary>
    /// <returns>The conventional controller's payload</returns>
    [HttpGet]
    public ActionResult<ConventionalEndpointResponse> Get()
    {
        return Ok(
            new ConventionalEndpointResponse
            {
                Value = "conventional",
                Area = RouteData.Values.TryGetValue("area", out var area) ? area as string : null,
                Controller = RouteData.Values.TryGetValue("controller", out var controller)
                    ? controller as string
                    : null,
                Key = RouteData.Values.TryGetValue("dynamicKey", out var key) ? key as string : null,
            }
        );
    }
}
