using Annium.Core.DependencyInjection;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Core.Mapper.Tests
{
    public class FieldMappingTest
    {
        [Fact]
        public void AssignmentMapping_Works()
        {
            // arrange
            var mapper = GetMapper();
            var value = new A { Text = "Some Text" };

            // act
            var result = mapper.Map<B>(value);

            // assert
            result.Ignored.IsEqual(0);
            result.LowerText.IsEqual("some text");
        }

        [Fact]
        public void ConstructorMapping_Works()
        {
            // arrange
            var mapper = GetMapper();
            var value = new A { Text = "Some Text" };

            // act
            var result = mapper.Map<C>(value);

            // assert
            result.Ignored.IsEqual(0);
            result.LowerText.IsEqual("some text");
        }

        private IMapper GetMapper() => new ServiceCollection()
            .AddMapper(autoload: false)
            .AddProfile(ConfigureProfile)
            .BuildServiceProvider()
            .GetRequiredService<IMapper>();

        private void ConfigureProfile(Profile p)
        {
            p.Map<A, B>()
                .Field(e => e.Text!.ToLower(), e => e.LowerText!)
                .Ignore(e => e.Ignored);
            p.Map<A, C>()
                .Field(e => e.Text!.ToLower(), c => c.LowerText!)
                .Ignore(e => e.Ignored);
        }

        private class A
        {
            public string? Text { get; set; }
        }

        private class B
        {
            public int Ignored { get; set; }

            public string? LowerText { get; set; }
        }

        private class C
        {
            public int Ignored { get; }

            public string? LowerText { get; }

            public C(int ignored, string? lowerText)
            {
                Ignored = ignored;
                LowerText = lowerText;
            }
        }
    }
}