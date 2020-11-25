using Annium.Extensions.Composition;
using Annium.Extensions.Composition.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddComposition(this IServiceContainer container)
        {
            container.AddAll()
                .AssignableTo(typeof(Composer<>))
                .Where(x => !x.IsGenericType)
                .AsInterfaces()
                .Scoped();

            container.Add(typeof(CompositionExecutor<>)).As(typeof(IComposer<>)).Scoped();

            return container;
        }
    }
}