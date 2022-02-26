using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Reflection.Tests.Types.Extensions;

public class NullableValueTypesExtensionTests
{
    [Fact]
    public void IsNotNullableValueType_OfNull_Throws()
    {
        //assert
        Wrap.It(() => (null as Type) !.IsNotNullableValueType()).Throws<ArgumentNullException>();
    }

    [Fact]
    public void IsNullableValueType_OfNull_Throws()
    {
        //assert
        Wrap.It(() => (null as Type) !.IsNullableValueType()).Throws<ArgumentNullException>();
    }

    [Fact]
    public void IsNotNullableValueType_Works()
    {
        //assert
        typeof(object).IsNotNullableValueType().IsFalse();
        typeof(bool).IsNotNullableValueType().IsTrue();
        typeof(bool?).IsNotNullableValueType().IsFalse();
    }

    [Fact]
    public void IsNullableValueType_Works()
    {
        //assert
        typeof(object).IsNullableValueType().IsFalse();
        typeof(bool).IsNullableValueType().IsFalse();
        typeof(bool?).IsNullableValueType().IsTrue();
    }
}