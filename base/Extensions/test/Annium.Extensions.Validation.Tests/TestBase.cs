using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.Validation.Tests
{
    public class TestBase
    {
        protected IValidator<T> GetValidator<T>() => new ServiceCollection()
            .AddValidation()
            .BuildServiceProvider()
            .GetRequiredService<IValidator<T>>();
    }
}