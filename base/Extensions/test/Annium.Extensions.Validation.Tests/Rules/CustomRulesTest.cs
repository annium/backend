using System.Threading.Tasks;
using Annium.Localization.Abstractions;
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
            var result = await validator.ValidateAsync(new Person());

            // assert
            result.HasErrors.IsTrue();
            result.LabeledErrors.Has(1);
            result.LabeledErrors.At(nameof(Person.Name)).At(0).IsEqual("Value is required");
        }

        [Fact]
        public async Task CustomAsyncRule_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var result = await validator.ValidateAsync(new Person() { Name = "ho" });

            // assert
            result.HasErrors.IsTrue();
            result.LabeledErrors.Has(1);
            result.LabeledErrors.At(nameof(Person.Name)).At(0).IsEqual($"{nameof(Person.Name)} value is too short");
        }

        private class Person
        {
            public string Name { get; set; }
        }

        private class PersonValidator : Validator<Person>
        {
            public PersonValidator(ILocalizer<PersonValidator> localizer) : base(localizer)
            {
                Field(p => p.Name)
                    .Required()
                    .Then()
                    .Add(async(context, value) =>
                    {
                        await Task.CompletedTask;
                        if (value.Length < 3)
                            context.Error("{0} value is too short", context.Field);
                    });
            }
        }
    }
}