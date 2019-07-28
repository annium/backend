using System;
using Annium.Testing;

namespace Annium.Extensions.Primitives.Tests
{
    public class StringExtensionsTest
    {
        [Fact]
        public void IsNullOrEmpty_WorksCorrectly()
        {
            (null as string).IsNullOrEmpty().IsTrue();
            "".IsNullOrEmpty().IsTrue();
            " ".IsNullOrEmpty().IsFalse();
        }

        [Fact]
        public void IsNullOrWhiteSpace_WorksCorrectly()
        {
            (null as string).IsNullOrWhiteSpace().IsTrue();
            "".IsNullOrWhiteSpace().IsTrue();
            " ".IsNullOrWhiteSpace().IsTrue();
        }

        [Fact]
        public void UpperFirst_WorksCorrectly()
        {
            (null as string).UpperFirst().IsEqual(null);
            "".UpperFirst().IsEqual(string.Empty);
            " ".UpperFirst().IsEqual(string.Empty);
            " ab ".UpperFirst().IsEqual("Ab");
            " aB ".UpperFirst().IsEqual("AB");
        }

        [Fact]
        public void LowerFirst_WorksCorrectly()
        {
            (null as string).LowerFirst().IsEqual(null);
            "".LowerFirst().IsEqual(string.Empty);
            " ".LowerFirst().IsEqual(string.Empty);
            " AB ".LowerFirst().IsEqual("aB");
            " Ab ".LowerFirst().IsEqual("ab");
        }

        [Fact]
        public void PascalCase_WorksCorrectly()
        {
            (null as string).PascalCase().IsEqual(null);
            "".PascalCase().IsEqual(string.Empty);
            " ".PascalCase().IsEqual(string.Empty);
            "0".PascalCase().IsEqual("0");
            "A B".PascalCase().IsEqual("AB");
            " - test that__".PascalCase().IsEqual("TestThat");
            "Foo Bar".PascalCase().IsEqual("FooBar");
            "_foo-bar_".PascalCase().IsEqual("FooBar");
            "FOO_BAR".PascalCase().IsEqual("FooBar");
            "FOo_BAr".PascalCase().IsEqual("FOoBAr");
            "andThatBAR0a00KBar0KK12312".PascalCase().IsEqual("AndThatBar0A00KBar0Kk12312");
        }

        [Fact]
        public void CamelCase_WorksCorrectly()
        {
            (null as string).CamelCase().IsEqual(null);
            "".CamelCase().IsEqual(string.Empty);
            " ".CamelCase().IsEqual(string.Empty);
            "A B".CamelCase().IsEqual("aB");
            " - test that__".CamelCase().IsEqual("testThat");
            "Foo Bar".CamelCase().IsEqual("fooBar");
            "_foo-bar_".CamelCase().IsEqual("fooBar");
            "FOO_BAR".CamelCase().IsEqual("fooBar");
            "FOo_BAr".CamelCase().IsEqual("fOoBAr");
            "andThatBAR0a00KBar0KK12312".CamelCase().IsEqual("andThatBar0A00KBar0Kk12312");
        }

        [Fact]
        public void KebabCase_WorksCorrectly()
        {
            (null as string).KebabCase().IsEqual(null);
            "".KebabCase().IsEqual(string.Empty);
            " ".KebabCase().IsEqual(string.Empty);
            "A B".KebabCase().IsEqual("a-b");
            " - test that__".KebabCase().IsEqual("test-that");
            "Foo Bar".KebabCase().IsEqual("foo-bar");
            "_foo-bar_".KebabCase().IsEqual("foo-bar");
            "FOO_BAR".KebabCase().IsEqual("foo-bar");
            "FOo_BAr".KebabCase().IsEqual("f-oo-b-ar");
            "andThatBAR0a00KBar0KK12312".KebabCase().IsEqual("and-that-bar0-a00-k-bar0-kk12312");
        }

        [Fact]
        public void SnakeCase_WorksCorrectly()
        {
            (null as string).SnakeCase().IsEqual(null);
            "".SnakeCase().IsEqual(string.Empty);
            " ".SnakeCase().IsEqual(string.Empty);
            "A B".SnakeCase().IsEqual("a_b");
            " - test that__".SnakeCase().IsEqual("test_that");
            "Foo Bar".SnakeCase().IsEqual("foo_bar");
            "_foo-bar_".SnakeCase().IsEqual("foo_bar");
            "FOO_BAR".SnakeCase().IsEqual("foo_bar");
            "FOo_BAr".SnakeCase().IsEqual("f_oo_b_ar");
            "andThatBAR0a00KBar0KK12312".SnakeCase().IsEqual("and_that_bar0_a00_k_bar0_kk12312");
        }

        [Fact]
        public void ToWords_WorksCorrectly()
        {
            (null as string).ToWords().Has(0);
        }

        [Fact]
        public void Repeat_WorksCorrectly()
        {
            // assert
            (null as string).Repeat(2).IsDefault();
            "demo".Repeat(-2).IsEqual("demo");
            "demo".Repeat(2).IsEqual("demodemo");
        }

        [Fact]
        public void FromHexStringToByteArray_Null_ThrowsArgumentNullOrReturnsFalse()
        {
            // arrange
            string str = null;

            // act
            var tryResult = str.TryFromHexStringToByteArray(out var byteArray);

            // assert
            ((Func<byte[]>) (() => str.FromHexStringToByteArray())).Throws<ArgumentNullException>();
            tryResult.IsFalse();
            byteArray.IsDefault();
        }

        [Fact]
        public void FromHexStringToByteArray_InvalidFormat_ThrowsFormatOrReturnsFalse()
        {
            // arrange
            var str = "a";

            // act
            var tryResult = str.TryFromHexStringToByteArray(out var byteArray);

            // assert
            ((Func<byte[]>) (() => str.FromHexStringToByteArray())).Throws<FormatException>();
            tryResult.IsFalse();
            byteArray.IsDefault();
        }

        [Fact]
        public void FromHexStringToByteArray_InvalidChars_ThrowsOverflowOrReturnsFalse()
        {
            // arrange
            var str1 = "ag";
            var str2 = "g0";

            // act
            var tryResult1 = str1.TryFromHexStringToByteArray(out var byteArray1);
            var tryResult2 = str2.TryFromHexStringToByteArray(out var byteArray2);

            // assert
            ((Func<byte[]>) (() => str1.FromHexStringToByteArray())).Throws<OverflowException>();
            ((Func<byte[]>) (() => str2.FromHexStringToByteArray())).Throws<OverflowException>();
            tryResult1.IsFalse();
            tryResult2.IsFalse();
            byteArray1.IsDefault();
            byteArray2.IsDefault();
        }

        [Fact]
        public void FromHexStringToByteArray_Valid_Works()
        {
            // arrange
            var str = "07DC22";

            // act
            var result = str.FromHexStringToByteArray();
            var tryResult = str.TryFromHexStringToByteArray(out var byteArray);

            // assert
            result.AsSpan().SequenceEqual(new byte[] { 7, 220, 34 }).IsTrue();
            tryResult.IsTrue();
            byteArray.AsSpan().SequenceEqual(new byte[] { 7, 220, 34 }).IsTrue();
        }
    }
}