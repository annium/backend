using System;

namespace Annium.AspNetCore.Extensions.Internal.DynamicControllers
{
    internal class DynamicControllerModel
    {
        public Type Type { get; }
        public string? Area { get; }
        public string Name { get; }
        public string Route { get; }

        public DynamicControllerModel(
            Type type,
            string? area,
            string name,
            string route
        )
        {
            Type = type;
            Area = area;
            Name = name;
            Route = route;
        }
    }
}
