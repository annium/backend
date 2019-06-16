using System;
using System.Threading.Tasks;
using Annium.Testing;

namespace Annium.Extensions.Validation.Tests
{
    public class ValidatorTest : TestBase
    {
        [Fact]
        public void Field_AccessorIsNull_ThrowsArgumentNullException()
        {
            // act
            ((Func<Validator<Bad>>) (() => GetValidator<Bad>())).Throws<ArgumentNullException>();
        }

        [Fact]
        public async Task Validation_NullWithoutLabel_UsesPlainError()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var result = await validator.Validate(null);

            // assert
            result.IsFailure.IsTrue();
            result.PlainErrors.Has(1);
            result.PlainErrors.At(0).IsEqual("Value is null");
        }

        [Fact]
        public async Task Validation_NullWithLabel_UsesLabelForLabeledError()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var result = await validator.Validate(null, "nested");

            // assert
            result.IsFailure.IsTrue();
            result.LabeledErrors.Has(1);
            result.LabeledErrors.At("nested").IsEqual("Value is null");
        }

        [Fact]
        public async Task Validation_WithoutLabel_UsesPropertyNameAsLabel()
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
        public async Task Validation_WithLabel_UsesLabelAndPropertyNameAsLabel()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var result = await validator.Validate(new Person(), "nested");

            // assert
            result.IsFailure.IsTrue();
            result.LabeledErrors.Has(1);
            result.LabeledErrors.At($"nested.{nameof(Person.Name)}").IsEqual("Value is required");
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

        private class Bad
        {
            public string Name { get; set; }
        }

        private class BadValidator : Validator<Bad>
        {
            public BadValidator()
            {
                Field<string>(null).IsRequired();
            }
        }
    }
}