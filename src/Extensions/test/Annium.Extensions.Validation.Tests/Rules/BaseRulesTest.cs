using System;
using System.Threading.Tasks;
using Annium.Testing;

namespace Annium.Extensions.Validation.Tests.Rules
{
    public class BaseRulesTest : TestBase
    {
        [Fact]
        public async Task Required_String_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { Name = "some" });
            var resultBad = await validator.Validate(new Person() { Name = " " });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.Name)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.Name)).IsEqual("Value is required");
        }

        [Fact]
        public async Task Required_NotString_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { Age = 1 });
            var resultBad = await validator.Validate(new Person());

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.Age)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.Age)).IsEqual("Value is required");
        }

        [Fact]
        public async Task EqualValue_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { Fixed = "fixed value" });
            var resultBad = await validator.Validate(new Person() { Fixed = "other value" });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.Fixed)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.Fixed)).IsEqual("Value is not equal to given");
        }

        [Fact]
        public async Task In_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { OneOf = "one" });
            var resultBad = await validator.Validate(new Person() { OneOf = "three" });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.OneOf)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.OneOf)).IsEqual("Value is not in given");
        }

        [Fact]
        public async Task EqualAccessor_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { Name = "some", SameAsName = "some" });
            var resultBad = await validator.Validate(new Person() { Name = "some", SameAsName = "other" });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.SameAsName)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.SameAsName)).IsEqual("Value is not equal to given");
        }

        [Fact]
        public async Task NotEqualValue_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { NotFixed = "other value" });
            var resultBad = await validator.Validate(new Person() { NotFixed = "fixed value" });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.NotFixed)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.NotFixed)).IsEqual("Value is equal to given");
        }

        [Fact]
        public async Task NotIn_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { NotOneOf = "three" });
            var resultBad = await validator.Validate(new Person() { NotOneOf = "one" });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.NotOneOf)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.NotOneOf)).IsEqual("Value is in given");
        }

        [Fact]
        public async Task NotEqualAccessor_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { Name = "some", NotSameAsName = "other" });
            var resultBad = await validator.Validate(new Person() { Name = "some", NotSameAsName = "some" });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.NotSameAsName)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.NotSameAsName)).IsEqual("Value is equal to given");
        }

        [Fact]
        public async Task MinMaxLength_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { MinMaxLength = "other" });
            var resultBadMin = await validator.Validate(new Person() { MinMaxLength = "x" });
            var resultBadMax = await validator.Validate(new Person() { MinMaxLength = "otherx" });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.MinMaxLength)).IsFalse();
            resultBadMin.LabeledErrors.At(nameof(Person.MinMaxLength)).IsEqual("Value length is less, than 2");
            resultBadMax.LabeledErrors.At(nameof(Person.MinMaxLength)).IsEqual("Value length is greater, than 5");
        }

        [Fact]
        public async Task MinLength_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { MinLength = "other" });
            var resultBad = await validator.Validate(new Person() { MinLength = "x" });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.MinLength)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.MinLength)).IsEqual("Value length is less, than 2");
        }

        [Fact]
        public async Task MaxLength_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { MaxLength = "other" });
            var resultBad = await validator.Validate(new Person() { MaxLength = "otherx" });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.MaxLength)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.MaxLength)).IsEqual("Value length is greater, than 5");
        }

        [Fact]
        public async Task Between_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { Between = 2 });
            var resultBadMin = await validator.Validate(new Person() { Between = 1 });
            var resultBadMax = await validator.Validate(new Person() { Between = 4 });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.Between)).IsFalse();
            resultBadMin.LabeledErrors.At(nameof(Person.Between)).IsEqual("Value is less, than given minimum");
            resultBadMax.LabeledErrors.At(nameof(Person.Between)).IsEqual("Value is greater, than given maximum");
        }

        [Fact]
        public async Task LessThan_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { LessThan = 2 });
            var resultBad = await validator.Validate(new Person() { LessThan = 3 });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.LessThan)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.LessThan)).IsEqual("Value is greater, than given maximum");
        }

        [Fact]
        public async Task LessThanOrEqual_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { LessThanOrEqual = 3 });
            var resultBad = await validator.Validate(new Person() { LessThanOrEqual = 4 });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.LessThanOrEqual)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.LessThanOrEqual)).IsEqual("Value is greater, than given maximum");
        }

        [Fact]
        public async Task GreaterThan_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { GreaterThan = 4 });
            var resultBad = await validator.Validate(new Person() { GreaterThan = 3 });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.GreaterThan)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.GreaterThan)).IsEqual("Value is less, than given minimum");
        }

        [Fact]
        public async Task GreaterThanOrEqual_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { GreaterThanOrEqual = 3 });
            var resultBad = await validator.Validate(new Person() { GreaterThanOrEqual = 2 });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.GreaterThanOrEqual)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.GreaterThanOrEqual)).IsEqual("Value is less, than given minimum");
        }

        [Fact]
        public async Task Regex_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { Regex = "yes" });
            var resultBad = await validator.Validate(new Person() { Regex = "yoda" });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.Regex)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.Regex)).IsEqual("Value doesn't match specified regex");
        }

        [Fact]
        public async Task Email_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { Email = "yes@xx" });
            var resultBad = await validator.Validate(new Person() { Email = "yoda@" });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.Email)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.Email)).IsEqual("Value is not an email");
        }

        [Fact]
        public void Enum_NotEnumType_ThrowsArgumentException()
        {
            // act
            ((Func<Validator<BadEnum>>) (() => GetValidator<BadEnum>())).Throws<ArgumentException>();
        }

        [Fact]
        public async Task Enum_Works()
        {
            // arrange
            var validator = GetValidator<Person>();

            // act
            var resultGood = await validator.Validate(new Person() { Enum = LogLevel.Debug });
            var resultBad = await validator.Validate(new Person() { Enum = (LogLevel) 0 });

            // assert
            resultGood.LabeledErrors.ContainsKey(nameof(Person.Enum)).IsFalse();
            resultBad.LabeledErrors.At(nameof(Person.Enum)).IsEqual("Value is not in expected range");
        }

        private class Person
        {
            public string Name { get; set; }
            public uint Age { get; set; }
            public string Fixed { get; set; }
            public string OneOf { get; set; }
            public string SameAsName { get; set; }
            public string NotFixed { get; set; }
            public string NotOneOf { get; set; }
            public string NotSameAsName { get; set; }
            public string MinMaxLength { get; set; }
            public string MinLength { get; set; }
            public string MaxLength { get; set; }
            public long Between { get; set; }
            public long LessThan { get; set; }
            public long LessThanOrEqual { get; set; }
            public long GreaterThan { get; set; }
            public long GreaterThanOrEqual { get; set; }
            public string Regex { get; set; }
            public string Email { get; set; }
            public LogLevel Enum { get; set; }
        }

        private enum LogLevel
        {
            Trace = 1,
            Debug = 2,
            Info = 3,
        }

        private class PersonValidator : Validator<Person>
        {
            public PersonValidator()
            {
                Field(p => p.Name).Required();
                Field(p => p.Age).Required();
                Field(p => p.Fixed).Equal("fixed value");
                Field(p => p.OneOf).In(new [] { "one", "two" });
                Field(p => p.SameAsName).Equal(p => p.Name);
                Field(p => p.NotFixed).NotEqual("fixed value");
                Field(p => p.NotOneOf).NotIn(new [] { "one", "two" });
                Field(p => p.NotSameAsName).NotEqual(p => p.Name);
                Field(p => p.MinMaxLength).Length(2, 5);
                Field(p => p.MinLength).MinLength(2);
                Field(p => p.MaxLength).MaxLength(5);
                Field(p => p.Between).Between(2, 3);
                Field(p => p.LessThan).LessThan(3);
                Field(p => p.LessThanOrEqual).LessThanOrEqual(3);
                Field(p => p.GreaterThan).GreaterThan(3);
                Field(p => p.GreaterThanOrEqual).GreaterThanOrEqual(3);
                Field(p => p.Regex).Matches("^\\w{2,3}$");
                Field(p => p.Email).Email();
                Field(p => p.Enum).Enum();
            }
        }

        private class BadEnum
        {
            public string Name { get; set; }
        }

        private class BadEnumValidator : Validator<BadEnum>
        {
            public BadEnumValidator()
            {
                Field(e => e.Name).Enum();
            }
        }
    }
}