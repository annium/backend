using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Annium.AspNetCore.Extensions.Internal.DynamicControllers
{
    internal class DynamicControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly IReadOnlyCollection<DynamicControllerModel> _models;

        public DynamicControllerFeatureProvider(
            IReadOnlyCollection<DynamicControllerModel> models
        )
        {
            _models = models;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var model in _models)
                feature.Controllers.Add(model.Type.GetTypeInfo());
        }
    }
}