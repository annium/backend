using System.Threading.Tasks;
using Annium.Testing;

namespace Annium.Extensions.Validation.Tests.Rules
{
    public class BaseRulesTest : TestBase
    {
        [Fact]
        public async Task IsRequired_ReferenceTypeNotNull_ReturnsFailure()
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

        private class Person
        {
            public string Name { get; set; }
        }

        private class PersonValidator : Validator<Person>
        {
            public PersonValidator()
            {
                Field(p => p.Name).IsRequired();
            }
        }
    }
}