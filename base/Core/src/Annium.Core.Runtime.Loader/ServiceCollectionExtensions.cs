using Annium.Core.Runtime.Loader;
using Annium.Core.Runtime.Loader.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceContainer AddAssemblyLoader(this IServiceContainer container)
        {
            container.Add<IAssemblyLoaderBuilder, AssemblyLoaderBuilder>().Transient();

            return container;
        }
    }
}