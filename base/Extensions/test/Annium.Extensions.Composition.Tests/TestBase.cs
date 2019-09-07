using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.Composition.Tests
{
    public class TestBase
    {
        protected IComposer<T> GetComposer<T>() where T : class => new ServiceCollection()
            .AddComposition()
            .AddLocalization(opts => opts.UseInMemoryStorage())
            .BuildServiceProvider()
            .GetRequiredService<IComposer<T>>();
    }
}