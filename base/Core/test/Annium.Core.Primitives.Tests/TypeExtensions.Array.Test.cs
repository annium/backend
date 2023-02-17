using System;
using System.Collections;
using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Primitives.Tests;

public class TypeArrayExtensionsTest
{
    [Fact]
    public void IsEnumerable_Ok()
    {
        typeof(string).IsEnumerable().IsFalse();
        typeof(Array).IsEnumerable().IsTrue();
        typeof(string[]).IsEnumerable().IsTrue();
        typeof(IEnumerable).IsEnumerable().IsTrue();
        typeof(IReadOnlyDictionary<,>).IsEnumerable().IsTrue();
    }

    [Fact]
    public void TryGetArrayElementType_Ok()
    {
        typeof(string).TryGetArrayElementType().IsDefault();
        typeof(Array).TryGetArrayElementType().IsDefault();
        typeof(string[]).TryGetArrayElementType().Is(typeof(string));
        typeof(IEnumerable).TryGetArrayElementType().IsDefault();
        typeof(IEnumerable<int>).TryGetArrayElementType().Is(typeof(int));
        typeof(IReadOnlyDictionary<,>).TryGetArrayElementType().IsNotDefault();
        typeof(IReadOnlyDictionary<,>).TryGetArrayElementType()?.GetGenericTypeDefinition().Is(typeof(KeyValuePair<,>));
    }
}