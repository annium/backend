using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;

namespace Annium.Extensions.Validation.Tests
{
    public class TestBase
    {
        protected IValidator<T> GetValidator<T>() => new ServiceContainer()
            .AddRuntimeTools(Assembly.GetCallingAssembly(), false, Assembly.GetCallingAssembly().ShortName())
            .AddValidation()
            .AddLocalization(opts => opts.UseInMemoryStorage())
            .BuildServiceProvider()
            .Resolve<IValidator<T>>();
    }
}