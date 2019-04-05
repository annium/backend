using System.Linq;
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
    }
}