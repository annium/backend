using System.Collections.Generic;
using Annium.Core.Runtime.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Runtime.Tests.Types;

public class TypeManagerTest
{
    [Fact]
    public void CanResolve_Works()
    {
        // arrange
        var manager = GetTypeManager();

        // assert
        manager.HasImplementations(typeof(A)).IsTrue();
        manager.HasImplementations(typeof(B)).IsFalse();
        manager.HasImplementations(typeof(C)).IsFalse();
        manager.HasImplementations(typeof(IEnumerable<>)).IsTrue();
    }

    [Fact]
    public void GetImplementations_ForAncestors_Works()
    {
        // arrange
        var manager = GetTypeManager();

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
        var manager = GetTypeManager();

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
        var manager = GetTypeManager();

        var implementations = manager.GetImplementations(typeof(GenericClass<,>));

        // assert
        implementations.Has(3);
        implementations.At(0).IsEqual(typeof(GenericClassDemoA<>));
        implementations.At(1).IsEqual(typeof(GenericClassDemoB<>));
        implementations.At(2).IsEqual(typeof(GenericClassDemoC));
    }

    [Fact]
    public void ResolveBySignature_FromInstance_Works()
    {
        // arrange
        var manager = GetTypeManager();
        var value = new { ForB = 5 };

        // act
        var result = manager.Resolve(value, typeof(A));

        // assert
        result.IsEqual(typeof(B));
    }

    [Fact]
    public void ResolveBySignature_FromSignature_Works()
    {
        // arrange
        var manager = GetTypeManager();

        // act
        var result = manager.ResolveBySignature(new[] { nameof(B.ForB) }, typeof(A), true);

        // assert
        result.IsEqual(typeof(B));
    }

    [Fact]
    public void ResolveByKey_NoDescendants_Throws()
    {
        // arrange
        var manager = GetTypeManager();
        var key = "key";

        // assert
        Wrap.It(() => manager.ResolveByKey(key, typeof(F))).Throws<TypeResolutionException>();
    }

    [Fact]
    public void ResolveByKey_Ambiguity_Throws()
    {
        // arrange
        var manager = GetTypeManager();
        var key = "F";

        // assert
        Wrap.It(() => manager.ResolveByKey(key, typeof(D))).Throws<TypeResolutionException>();
    }

    [Fact]
    public void ResolveByKey_Normally_Works()
    {
        // arrange
        var manager = GetTypeManager();
        var key = "E";

        // act
        var result = manager.ResolveByKey(key, typeof(D));

        // assert
        result.IsEqual(typeof(E));
    }

    [Fact]
    public void Resolve_ById_Works()
    {
        // arrange
        var manager = GetTypeManager();
        var source = new L { Type = typeof(K).GetIdString() };

        // act
        var result = manager.Resolve(source, typeof(H));

        // assert
        result.IsEqual(typeof(K));
    }

    [Fact]
    public void Resolve_ByKey_Works()
    {
        // arrange
        var manager = GetTypeManager();
        object source = new E();

        // act
        var result = manager.Resolve(source, typeof(D));

        // assert
        result.IsEqual(typeof(E));
    }

    [Fact]
    public void Resolve_BySignature_Works()
    {
        // arrange
        var manager = GetTypeManager();
        object source = new B();

        // act
        var result = manager.Resolve(source, typeof(A));

        // assert
        result.IsEqual(typeof(B));
    }

    private ITypeManager GetTypeManager() => TypeManager.GetInstance(GetType().Assembly);

    private class A
    {
    }

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
        [ResolutionKey]
        public string Type { get; }

        protected D(string type)
        {
            Type = type;
        }
    }

    [ResolutionKeyValue(nameof(E))]
    private class E : D
    {
        public E() : base(nameof(E))
        {
        }
    }

    [ResolutionKeyValue(nameof(F))]
    private class F : D
    {
        public F() : base(nameof(F))
        {
        }
    }

    [ResolutionKeyValue(nameof(F))]
    private class G : D
    {
        public G() : base(nameof(F))
        {
        }
    }

    private class H
    {
        [ResolutionId]
        public string Type => GetType().GetIdString();
    }

    private class J : H
    {
    }

    private class K : H
    {
    }

    private record L
    {
        [ResolutionId]
        public string Type { get; set; } = string.Empty;
    }

    private interface IGenericInterface<T1, T2>
    {
    }

    private class GenericInterfaceDemoA<T> : IGenericInterface<T, int>
    {
    }

    private class GenericInterfaceDemoB<T> : IGenericInterface<T, long>
    {
    }

    private class GenericInterfaceDemoC : IGenericInterface<string, bool>
    {
    }

    private class GenericClass<T1, T2>
    {
    }

    private class GenericClassDemoA<T> : GenericClass<T, int>
    {
    }

    private class GenericClassDemoB<T> : GenericClass<T, long>
    {
    }

    private class GenericClassDemoC : GenericClass<string, bool>
    {
    }
}