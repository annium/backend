using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.Validation.Tests
{
    public class TestBase
    {
        protected IValidator<T> GetValidator<T>() => new ServiceCollection()
            .AddValidation()
            .AddLocalization(opts => opts.UseInMemoryStorage())
            .BuildServiceProvider()
            .GetRequiredService<IValidator<T>>();
    }
}