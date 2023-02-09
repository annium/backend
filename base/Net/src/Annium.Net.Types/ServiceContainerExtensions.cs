// ReSharper disable CheckNamespace

using Annium.Net.Types;
using Annium.Net.Types.Internal;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddModelMapper(this IServiceContainer container)
    {
        container.Add<IModelMapper, ModelMapper>().Transient();

        return container;
    }
}