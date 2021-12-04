using System.Reflection;
using Annium.Core.DependencyInjection;

namespace Annium.Extensions.Validation.Tests;

public class TestBase
{
    protected IValidator<T> GetValidator<T>() => new ServiceContainer()
        .AddRuntimeTools(Assembly.GetCallingAssembly(), false)
        .AddValidation()
        .AddLocalization(opts => opts.UseInMemoryStorage())
        .BuildServiceProvider()
        .Resolve<IValidator<T>>();
}