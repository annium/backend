using System;
using System.Threading.Tasks;
using Annium.Testing;

namespace Annium.Extensions.Composition.Tests
{
    public class ComposerTest : TestBase
    {
        [Fact]
        public void Field_AccessorIsNull_ThrowsArgumentNullException()
        {
            // act
            ((Func<IComposer<Bad>>) (() => GetComposer<Bad>())).Throws<ArgumentNullException>();
        }

        [Fact]
        public async Task Composition_NullWithoutLabel_UsesPlainError()
        {
            // arrange
            var composer = GetComposer<Person>();

            // act
            var result = await composer.ComposeAsync(null);

            // assert
            result.HasErrors.IsTrue();
            result.PlainErrors.Has(1);
            result.PlainErrors.At(0).IsEqual("Value is null");
        }

        [Fact]
        public async Task Composition_NullWithLabel_UsesLabelForLabeledError()
        {
            // arrange
            var composer = GetComposer<Person>();

            // act
            var result = await composer.ComposeAsync(null, "nested");

            // assert
            result.HasErrors.IsTrue();
            result.LabeledErrors.Has(1);
            result.LabeledErrors.At("nested").At(0).IsEqual("Value is null");
        }

        [Fact]
        public async Task Composition_WithoutLabel_UsesPropertyNameAsLabel()
        {
            // arrange
            var data = new Person();
            var composer = GetComposer<Person>();

            // act
            var result = await composer.ComposeAsync(data);

            // assert
            result.HasErrors.IsFalse();
            data.Name.IsEqual(nameof(Person.Name));
        }

        [Fact]
        public async Task Composition_CompoundThroughInterfaces()
        {
            // arrange
            var data = new User();
            var composer = GetComposer<User>();

            // act
            var result = await composer.ComposeAsync(data);

            // assert
            result.HasErrors.IsFalse();
            data.Email.IsEqual(nameof(User.Email));
            data.Login.IsEqual(nameof(User.Login));
        }

        private class User : IEmail, ILogin
        {
            public string Email { get; set; }
            public string Login { get; set; }
        }

        private interface IEmail
        {
            string Email { get; set; }
        }

        private interface ILogin
        {
            string Login { get; set; }
        }

        private class EmailComposer : Composer<IEmail>
        {
            public EmailComposer()
            {
                Field(p => p.Email).LoadWith(ctx => ctx.Label);
            }
        }

        private class LoginComposer : Composer<ILogin>
        {
            public LoginComposer()
            {
                Field(p => p.Login).LoadWith(ctx => ctx.Label);
            }
        }

        private class Person
        {
            public string Name { get; set; }
        }

        private class PersonComposer : Composer<Person>
        {
            public PersonComposer()
            {
                Field(p => p.Name).LoadWith(ctx => ctx.Label);
            }
        }

        private class Bad
        {
            public string Name { get; set; }
        }

        private class BadComposer : Composer<Bad>
        {
            public BadComposer()
            {
                Field<string>(null).LoadWith(ctx => ctx.Label);
            }
        }
    }
}