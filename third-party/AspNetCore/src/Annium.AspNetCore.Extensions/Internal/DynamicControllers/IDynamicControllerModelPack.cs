using Microsoft.AspNetCore.Mvc;

namespace Annium.AspNetCore.Extensions.Internal.DynamicControllers
{
    public interface IDynamicControllerModelPack
    {
        IDynamicControllerModelPack Setup(string? area, string? key);
        IDynamicControllerModelPack Add<T>(string name, string route) where T : ControllerBase;
    }
}