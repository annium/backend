using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Types;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IBulkRegistrationBuilderBase AddAll(this IServiceContainer container)
            => container.Add(container.GetTypeManager().Types.AsEnumerable());

        public static IBulkRegistrationBuilderBase AddAll(this IServiceContainer container, Assembly assembly, bool tryLoadReferences)
            => container.Add(TypeManager.GetInstance(assembly, tryLoadReferences).Types.AsEnumerable());

        public static IBulkRegistrationBuilderBase AddAll(this IServiceContainer container, Assembly assembly)
            => container.Add(assembly.GetTypes().AsEnumerable());
    }
}