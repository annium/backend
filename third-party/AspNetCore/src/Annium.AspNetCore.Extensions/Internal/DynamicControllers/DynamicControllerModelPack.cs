using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Annium.AspNetCore.Extensions.Internal.DynamicControllers
{
    internal class DynamicControllerModelPack : IDynamicControllerModelPack
    {
        internal IReadOnlyCollection<DynamicControllerModel> Models => _models;
        private string? _area;
        private string? _key;
        private readonly List<DynamicControllerModel> _models = new();

        public IDynamicControllerModelPack Setup(string? area, string? key)
        {
            _area = area;
            _key = key;

            return this;
        }

        public IDynamicControllerModelPack Add<T>(string name, string route)
            where T : ControllerBase
        {
            _models.Add(new DynamicControllerModel(typeof(T), _area, _key, name, route));

            return this;
        }
    }
}