using Annium.Extensions.Shell;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddShell(this IServiceContainer container)
        {
            container.Add<IShell, Shell>().Singleton();

            return container;
        }
    }
}