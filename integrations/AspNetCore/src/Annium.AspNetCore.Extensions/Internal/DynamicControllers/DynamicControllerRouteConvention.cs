using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Annium.AspNetCore.Extensions.Internal.DynamicControllers;

/// <summary>
/// Route convention that configures routing for dynamic controllers
/// </summary>
internal class DynamicControllerRouteConvention : IControllerModelConvention
{
    /// <summary>
    /// The collection of dynamic controller models for route configuration
    /// </summary>
    private readonly IReadOnlyCollection<DynamicControllerModel> _models;

    /// <summary>
    /// Initializes a new instance of the DynamicControllerRouteConvention class
    /// </summary>
    /// <param name="models">The collection of dynamic controller models</param>
    public DynamicControllerRouteConvention(IReadOnlyCollection<DynamicControllerModel> models)
    {
        _models = models;
    }

    /// <summary>
    /// Applies the route convention to the specified controller model
    /// </summary>
    /// <param name="controller">The controller model to configure</param>
    public void Apply(ControllerModel controller)
    {
        var model = _models.SingleOrDefault(x => x.Type == controller.ControllerType);
        if (model is null)
            return;

        var area = model.Area;
        var name = model.Name;
        var key = model.Key;
        var route = model.Area.IsNullOrWhiteSpace() ? model.Route : $"{area}/{model.Route}";

        controller.RouteValues["area"] = area;
        controller.RouteValues["controller"] = name;
        controller.RouteValues["dynamicKey"] = key;
        controller.Selectors.Add(
            new SelectorModel { AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(route)) }
        );
    }
}
