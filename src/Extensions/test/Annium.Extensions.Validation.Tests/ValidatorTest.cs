using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.Validation.Tests
{
    public class ValidatorTest
    {
        protected Validator<T> GetValidator<T>() => new ServiceCollection()
            .AddValidation()
            .BuildServiceProvider()
            .GetRequiredService<Validator<T>>();
    }
}