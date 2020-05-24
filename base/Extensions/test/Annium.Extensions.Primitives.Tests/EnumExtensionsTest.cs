using System.ComponentModel;
using System;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Primitives.Tests
{
    public class EnumExtensionsTest
    {
        [Fact]
        public void ParseEnum_NoDefault_Works()
        {
            // arrange
            var name = "one";
            var desc = "A";
            var value = "1";
            var invalid = "5";

            // assert
            name.ParseEnum<TestEnum>().IsEqual(TestEnum.One);
            desc.ParseEnum<TestEnum>().IsEqual(TestEnum.One);
            value.ParseEnum<TestEnum>().IsEqual(TestEnum.One);
            ((Func<TestEnum>)(() => invalid.ParseEnum<TestEnum>())).Throws<ArgumentException>();
        }

        [Fact]
        public void ParseFlags_NoDefault_Works()
        {
            // arrange
            var valid = "one | b";
            var invalid = "5, two";

            // assert
            valid.ParseFlags<TestEnum>('|').IsEqual(TestEnum.One | TestEnum.Two);
            string.Empty.ParseFlags<TestEnum>('|').IsEqual(TestEnum.None);
            ((Func<TestEnum>)(() => invalid.ParseFlags<TestEnum>(','))).Throws<ArgumentException>();
        }

        [Fact]
        public void ParseEnum_Default_Works()
        {
            // arrange
            var name = "one";
            var desc = "A";
            var value = "1";
            var invalid = "5";

            // assert
            name.ParseEnum(TestEnum.None).IsEqual(TestEnum.One);
            desc.ParseEnum(TestEnum.None).IsEqual(TestEnum.One);
            value.ParseEnum(TestEnum.None).IsEqual(TestEnum.One);
            invalid.ParseEnum(TestEnum.None).IsEqual(TestEnum.None);
        }

        [Fact]
        public void ParseFlags_Default_Works()
        {
            // arrange
            var valid = "one | b";
            var invalid = "5, two";

            // assert
            valid.ParseFlags('|', TestEnum.One).IsEqual(TestEnum.One | TestEnum.Two);
            invalid.ParseFlags(',', TestEnum.One).IsEqual(TestEnum.One | TestEnum.Two);
        }

        [Flags]
        private enum TestEnum
        {
            [Description("empty")]
            None,
            [Description("a")]
            One,
            [Description("b")]
            Two
        }
    }
}