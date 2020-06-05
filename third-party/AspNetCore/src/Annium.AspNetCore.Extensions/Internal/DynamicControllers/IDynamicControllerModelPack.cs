using Microsoft.AspNetCore.Mvc;

namespace Annium.AspNetCore.Extensions.Internal.DynamicControllers
{
    public interface IDynamicControllerModelPack
    {
        IDynamicControllerModelPack SetArea(string area);
        IDynamicControllerModelPack Add<T>(string name, string route) where T : ControllerBase;
    }
}
