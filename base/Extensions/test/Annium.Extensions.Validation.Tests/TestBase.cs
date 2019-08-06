using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.Validation.Tests
{
    public class TestBase
    {
        protected Validator<T> GetValidator<T>() => new ServiceCollection()
            .AddValidation()
            .BuildServiceProvider()
            .GetRequiredService<Validator<T>>();
    }
}