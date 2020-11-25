using Annium.Extensions.Arguments;
using Annium.Extensions.Arguments.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static void AddArguments(this IServiceContainer container)
        {
            container.Add<IArgumentProcessor, ArgumentProcessor>().Singleton();
            container.Add<IConfigurationBuilder, ConfigurationBuilder>().Singleton();
            container.Add<IConfigurationProcessor, ConfigurationProcessor>().Singleton();
            container.Add<IHelpBuilder, HelpBuilder>().Singleton();
            container.Add<Commander>().Singleton();
            container.Add<Root>().Singleton();
        }
    }
}