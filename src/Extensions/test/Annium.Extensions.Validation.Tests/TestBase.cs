using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.Validation.Tests
{
    public class TestBase
    {
        protected Validator<T> GetValidator<T>() => new ServiceCollection()
            .AddValidation()
            .BuildServiceProvider()
            .GetRequiredService<Validator<T>>();

        private class Person
        {
            public string Name { get; set; }
        }

        private class PersonValidator : Validator<Person>
        {
            public PersonValidator()
            {
                Field<string>(null).IsRequired();
            }
        }
    }
}