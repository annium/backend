using System;
using System.Collections.Generic;
using System.IO;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Reflection.Tests.Types.Extensions;

public class CanBeUsedAsExtensionTest
{
    [Fact]
    public void TypeNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.CanBeUsedAs(typeof(object))).Throws<ArgumentNullException>();
    }

    [Fact]
    public void TargetNull_Throws()
    {
        // assert
        Wrap.It(() => (typeof(object)).CanBeUsedAs((null as Type)!)).Throws<ArgumentNullException>();
    }

    [Fact]
    public void TypeContainsGenericParameters_Throws()
    {
        // assert
        Wrap.It(() => typeof(List<>).CanBeUsedAs(typeof(object))).Throws<InvalidOperationException>();
    }

    [Fact]
    public void TargetContainsParameters_Throws()
    {
        // assert
        Wrap.It(() => typeof(List<int>).CanBeUsedAs(typeof(object))).Throws<InvalidOperationException>();
    }

    [Fact]
    public void NotMatchingConstraints_ReturnsFalse()
    {
        // assert
        typeof(Stream).CanBeUsedAs(typeof(IEquatableConstraint<>).GetGenericArguments()[0]).IsFalse();
    }

    [Fact]
    public void MatchingConstraints_ReturnsTrue()
    {
        // assert
        typeof(string).CanBeUsedAs(typeof(IEquatableConstraint<>).GetGenericArguments()[0]).IsTrue();
        typeof(object).CanBeUsedAs(typeof(List<>).GetGenericArguments()[0]).IsTrue();
    }

    private interface IEquatableConstraint<T> where T : IEquatable<T>
    {
    }
}