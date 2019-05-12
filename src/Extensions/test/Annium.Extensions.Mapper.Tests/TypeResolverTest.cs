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
        public void ResolveBySignature_NoDescendants_Throws()
        {
            // arrange
            var resolver = TypeResolver.Instance;
            var value = new { ForB = 5 };

            // assert
            ((Func<Type>) (() => resolver.ResolveBySignature(value, typeof(B), false))).Throws<MappingException>();
        }

        [Fact]
        public void ResolveBySignature_FromInstance_Works()
        {
            // arrange
            var resolver = TypeResolver.Instance;
            var value = new { ForB = 5 };

            // act
            var result = resolver.ResolveBySignature(value, typeof(A), true);

            // assert
            result.IsEqual(typeof(B));
        }

        [Fact]
        public void ResolveBySignature_FromSignature_Works()
        {
            // arrange
            var resolver = TypeResolver.Instance;

            // act
            var result = resolver.ResolveBySignature(new [] { nameof(B.ForB) }, typeof(A), true);

            // assert
            result.IsEqual(typeof(B));
        }

        [Fact]
        public void ResolveByKey_NoDescendants_Throws()
        {
            // arrange
            var resolver = TypeResolver.Instance;
            var key = "key";

            // assert
            ((Func<Type>) (() => resolver.ResolveByKey(key, typeof(F)))).Throws<MappingException>();
        }

        [Fact]
        public void ResolveByKey_Ambiguity_Throws()
        {
            // arrange
            var resolver = TypeResolver.Instance;
            var key = "F";

            // assert
            ((Func<Type>) (() => resolver.ResolveByKey(key, typeof(D)))).Throws<MappingException>();
        }

        [Fact]
        public void ResolveByKey_Normally_Works()
        {
            // arrange
            var resolver = TypeResolver.Instance;
            var key = "E";

            // act
            var result = resolver.ResolveByKey(key, typeof(D));

            // assert
            result.IsEqual(typeof(E));
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

        private class D
        {
            [ResolveField]
            public string Type { get; set; }
        }

        [ResolveKey(nameof(E))]
        private class E : D { }

        [ResolveKey(nameof(F))]
        private class F : D { }

        [ResolveKey(nameof(F))]
        private class X : D { }
    }
}