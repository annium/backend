using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Reflection.Tests.Types
{
    public class TypeHelperTest
    {
        [Fact]
        public void ResolveProperties_Multiple_Works()
        {
            // act
            var properties = TypeHelper.ResolveProperties<B>(x => new { x.InnerOne.One, x.InnerTwo });

            // assert
            properties.Has(2);
            properties.At(0).IsEqual(typeof(A).GetProperty(nameof(A.One)));
            properties.At(1).IsEqual(typeof(B).GetProperty(nameof(B.InnerTwo)));
        }

        [Fact]
        public void ResolveProperty_Unary_Works()
        {
            // assert
            TypeHelper.ResolveProperty<A>(x => x.Two).IsEqual(typeof(A).GetProperty(nameof(A.Two)));
        }

        [Fact]
        public void ResolveProperty_Inner_Works()
        {
            // assert
            TypeHelper.ResolveProperty<B>(x => x.InnerTwo.Two).IsEqual(typeof(A).GetProperty(nameof(A.Two)));
        }

        [Fact]
        public void ResolveProperty_NotProperty_Fails()
        {
            // assert
            ((Action) (() => TypeHelper.ResolveProperty<B>(x => x.InnerTwo.ToString()!))).Throws<ArgumentException>();
        }

        private class B
        {
            public A InnerOne { get; set; } = null!;
            public A InnerTwo { get; set; } = null!;
        }

        private class A
        {
            public string One { get; set; } = null!;
            public string Two { get; set; } = null!;
        }
    }
}