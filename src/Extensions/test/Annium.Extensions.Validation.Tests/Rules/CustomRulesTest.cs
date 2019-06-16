using System.Threading.Tasks;
using Annium.Testing;

namespace Annium.Extensions.Validation.Tests.Rules
{
    public class CustomRulesTest : TestBase
    {
        [Fact]
        public async Task ThenRule_ImplementsShortCircuit()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var result = await validator.Validate(new Person());

            // assert
            result.IsFailure.IsTrue();
            result.LabeledErrors.Has(1);
            result.LabeledErrors.At(nameof(Person.Name)).IsEqual("Value is required");
        }

        [Fact]
        public async Task CustomAsyncRule_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var result = await validator.Validate(new Person() { Name = "ho" });

            // assert
            result.IsFailure.IsTrue();
            result.LabeledErrors.Has(1);
            result.LabeledErrors.At(nameof(Person.Name)).IsEqual($"{nameof(Person.Name)} value is too short");
        }

        private class Person
        {
            public string Name { get; set; }
        }

        private class PersonValidator : Validator<Person>
        {
            public PersonValidator()
            {
                Field(p => p.Name)
                    .IsRequired()
                    .Then()
                    .Add(async(context, value) =>
                    {
                        await Task.CompletedTask;
                        if (value.Length < 3)
                            context.Error($"{context.Field} value is too short");
                    });
            }
        }
    }
}