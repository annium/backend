using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Annium.AspNetCore.Extensions.Internal.DynamicControllers;

internal class DynamicControllerRouteConvention : IControllerModelConvention
{
    private readonly IReadOnlyCollection<DynamicControllerModel> _models;

    public DynamicControllerRouteConvention(IReadOnlyCollection<DynamicControllerModel> models)
    {
        _models = models;
    }

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
