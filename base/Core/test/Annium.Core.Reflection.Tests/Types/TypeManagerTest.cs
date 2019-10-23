using System;
using System.Collections.Generic;
using Annium.Core.Reflection;
using Annium.Testing;

namespace Annium.Core.Reflection.Tests.Types
{
    public class TypeManagerTest
    {
        [Fact]
        public void CanResolve_Works()
        {
            // arrange
            var manager = TypeManager.Instance;

            // assert
            manager.CanResolve(typeof(A)).IsTrue();
            manager.CanResolve(typeof(B)).IsFalse();
            manager.CanResolve(typeof(C)).IsFalse();
            manager.CanResolve(typeof(IEnumerable<>)).IsTrue();
        }

        [Fact]
        public void GetImplementations_ForAncestors_Works()
        {
            // arrange
            var manager = TypeManager.Instance;

            var implementations = manager.GetImplementations(typeof(A));

            // assert
            implementations.Has(2);
            implementations.At(0).IsEqual(typeof(B));
            implementations.At(1).IsEqual(typeof(C));
        }

        [Fact]
        public void GetImplementations_ForGenericInterfaceDefinitions_Works()
        {
            // arrange
            var manager = TypeManager.Instance;

            var implementations = manager.GetImplementations(typeof(IGenericInterface<,>));

            // assert
            implementations.Has(3);
            implementations.At(0).IsEqual(typeof(GenericInterfaceDemoA<>));
            implementations.At(1).IsEqual(typeof(GenericInterfaceDemoB<>));
            implementations.At(2).IsEqual(typeof(GenericInterfaceDemoC));
        }

        [Fact]
        public void GetImplementations_ForGenericClassDefinitions_Works()
        {
            // arrange
            var manager = TypeManager.Instance;

            var implementations = manager.GetImplementations(typeof(GenericClass<,>));

            // assert
            implementations.Has(3);
            implementations.At(0).IsEqual(typeof(GenericClassDemoA<>));
            implementations.At(1).IsEqual(typeof(GenericClassDemoB<>));
            implementations.At(2).IsEqual(typeof(GenericClassDemoC));
        }

        [Fact]
        public void ResolveBySignature_NoDescendants_Throws()
        {
            // arrange
            var manager = TypeManager.Instance;
            var value = new { ForB = 5 };

            // assert
            ((Func<Type>) (() => manager.ResolveBySignature(value, typeof(B), false) !)).Throws<TypeResolutionException>();
        }

        [Fact]
        public void ResolveBySignature_FromInstance_Works()
        {
            // arrange
            var manager = TypeManager.Instance;
            var value = new { ForB = 5 };

            // act
            var result = manager.ResolveBySignature(value, typeof(A), true);

            // assert
            result.IsEqual(typeof(B));
        }

        [Fact]
        public void ResolveBySignature_FromSignature_Works()
        {
            // arrange
            var manager = TypeManager.Instance;

            // act
            var result = manager.ResolveBySignature(new [] { nameof(B.ForB) }, typeof(A), true);

            // assert
            result.IsEqual(typeof(B));
        }

        [Fact]
        public void ResolveByKey_NoDescendants_Throws()
        {
            // arrange
            var manager = TypeManager.Instance;
            var key = "key";

            // assert
            ((Func<Type>) (() => manager.ResolveByKey(key, typeof(F)))).Throws<TypeResolutionException>();
        }

        [Fact]
        public void ResolveByKey_Ambiguity_Throws()
        {
            // arrange
            var manager = TypeManager.Instance;
            var key = "F";

            // assert
            ((Func<Type>) (() => manager.ResolveByKey(key, typeof(D)))).Throws<TypeResolutionException>();
        }

        [Fact]
        public void ResolveByKey_Normally_Works()
        {
            // arrange
            var manager = TypeManager.Instance;
            var key = "E";

            // act
            var result = manager.ResolveByKey(key, typeof(D));

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
            public string Type { get; }

            protected D(string type)
            {
                Type = type;
            }
        }

        [ResolveKey(nameof(E))]
        private class E : D { public E() : base(nameof(E)) { } }

        [ResolveKey(nameof(F))]
        private class F : D { public F() : base(nameof(F)) { } }

        [ResolveKey(nameof(F))]
        private class X : D { public X() : base(nameof(F)) { } }

        private interface IGenericInterface<T1, T2> { }
        private class GenericInterfaceDemoA<T> : IGenericInterface<T, int> { }
        private class GenericInterfaceDemoB<T> : IGenericInterface<T, long> { }
        private class GenericInterfaceDemoC : IGenericInterface<string, bool> { }

        private class GenericClass<T1, T2> { }
        private class GenericClassDemoA<T> : GenericClass<T, int> { }
        private class GenericClassDemoB<T> : GenericClass<T, long> { }
        private class GenericClassDemoC : GenericClass<string, bool> { }
    }
}