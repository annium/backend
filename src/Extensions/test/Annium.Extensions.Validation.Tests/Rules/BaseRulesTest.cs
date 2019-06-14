using Annium.Testing;

namespace Annium.Extensions.Validation.Tests.Rules
{
    public class BaseRulesTest : ValidatorTest
    {
        [Fact]
        public void IsRequired_ReferenceTypeNull_ReturnsFailure()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var result = validator.Validate(null);

            // assert
            result.IsFailure.IsTrue();
            result.PlainErrors.Has(1);
            result.PlainErrors.At(0).IsEqual("Value is null");
        }

        [Fact]
        public void IsRequired_ReferenceTypeNotNull_ReturnsFailure()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var result = validator.Validate(new Person());

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