using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;

namespace Annium.Extensions.Composition.Tests
{
    public class TestBase
    {
        protected IComposer<T> GetComposer<T>() where T : class => new ServiceContainer()
            .AddRuntimeTools(Assembly.GetCallingAssembly(), false, Assembly.GetCallingAssembly().ShortName())
            .AddComposition()
            .AddLocalization(opts => opts.UseInMemoryStorage())
            .BuildServiceProvider()
            .Resolve<IComposer<T>>();
    }
}