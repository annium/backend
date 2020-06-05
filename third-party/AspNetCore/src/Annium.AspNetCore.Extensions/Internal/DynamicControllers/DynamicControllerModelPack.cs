using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Annium.AspNetCore.Extensions.Internal.DynamicControllers
{
    internal class DynamicControllerModelPack : IDynamicControllerModelPack
    {
        internal IReadOnlyCollection<DynamicControllerModel> Models => _models;
        private string? _area;
        private readonly List<DynamicControllerModel> _models = new List<DynamicControllerModel>();

        public IDynamicControllerModelPack SetArea(string? area)
        {
            _area = area;

            return this;
        }

        public IDynamicControllerModelPack Add<T>(string name, string route) where T : ControllerBase
        {
            _models.Add(new DynamicControllerModel(typeof(T), _area, name, route));

            return this;
        }
    }
}
