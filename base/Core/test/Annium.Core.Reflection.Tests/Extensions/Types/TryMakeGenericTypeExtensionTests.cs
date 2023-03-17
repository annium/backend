using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Reflection.Tests.Extensions.Types;

public class TryMakeGenericTypeExtensionTests
{
    [Fact]
    public void TryMakeGenericType_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.TryMakeGenericType(out _)).Throws<ArgumentNullException>();
    }

    [Fact]
    public void ITryMakeGenericType_Works()
    {
        // assert
        typeof(Demo<>).TryMakeGenericType(out Type? result).IsFalse();
        result.IsDefault();

        typeof(Demo<>).TryMakeGenericType(out result, typeof(bool)).IsFalse();
        result.IsDefault();

        typeof(Demo<>).TryMakeGenericType(out result, typeof(object)).IsTrue();
        result.Is(typeof(Demo<object>));
    }

    private class Demo<T> where T : class
    {
    }
}