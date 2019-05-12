using System;
using Annium.Testing;

namespace Annium.Extensions.Mapper.Tests
{
    public class TypeResolverTest
    {
        [Fact]
        public void CanResolve_Works()
        {
            // arrange
            var resolver = TypeResolver.Instance;

            // assert
            resolver.CanResolve(typeof(A)).IsTrue();
            resolver.CanResolve(typeof(B)).IsFalse();
            resolver.CanResolve(typeof(C)).IsFalse();
        }

        [Fact]
        public void SignatureResolution_Normally_Works()
        {
            // arrange
            var resolver = TypeResolver.Instance;
            var value = new { ForB = 5 };

            // act
            var result = resolver.ResolveBySignature(value, typeof(A));

            // assert
            result.IsEqual(typeof(B));
        }

        [Fact]
        public void SignatureResolution_NoDescendants_Throws()
        {
            // arrange
            var resolver = TypeResolver.Instance;
            var value = new { ForB = 5 };

            // assert
            ((Func<Type>) (() => resolver.ResolveBySignature(value, typeof(B)))).Throws<MappingException>();
        }

        private class A { }

        private class B : A
        {
            public int ForB { get; set; }
        }

        private class C : A
        {
            public int ForC { get; set; }
        }
    }
}