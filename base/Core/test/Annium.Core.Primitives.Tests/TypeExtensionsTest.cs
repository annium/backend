using System;
using System.Collections;
using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Primitives.Tests;

public class TypeExtensionsTest
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
    public void DefaultValue_Ok()
    {
        typeof(int).DefaultValue().Is(0);
        typeof(string[]).DefaultValue().Is(null);
        typeof(string).DefaultValue().Is(null);
    }
}