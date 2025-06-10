using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Annium.AspNetCore.Extensions.Internal.DynamicControllers;

/// <summary>
/// Feature provider that adds dynamic controllers to the MVC application
/// </summary>
internal class DynamicControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
    /// <summary>
    /// The collection of dynamic controller models to add to the application
    /// </summary>
    private readonly IReadOnlyCollection<DynamicControllerModel> _models;

    /// <summary>
    /// Initializes a new instance of the DynamicControllerFeatureProvider class
    /// </summary>
    /// <param name="models">The collection of dynamic controller models to add</param>
    public DynamicControllerFeatureProvider(IReadOnlyCollection<DynamicControllerModel> models)
    {
        _models = models;
    }

    /// <summary>
    /// Populates the controller feature with dynamic controllers
    /// </summary>
    /// <param name="parts">The application parts</param>
    /// <param name="feature">The controller feature to populate</param>
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        foreach (var model in _models)
            feature.Controllers.Add(model.Type.GetTypeInfo());
    }
}
