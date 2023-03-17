using System;
using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Reflection.Tests.Extensions.Types;

public class IsCuriouslyRecurringGenericParameterExtensionTests
{
    [Fact]
    public void IsCuriouslyRecurringGenericParameter_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.IsCuriouslyRecurringGenericParameter()).Throws<ArgumentNullException>();
    }

    [Fact]
    public void IsCuriouslyRecurringGenericParameter_Works()
    {
        // assert
        typeof(bool).IsCuriouslyRecurringGenericParameter().IsFalse();
        typeof(IEnumerable<>).GetGenericArguments()[0].IsCuriouslyRecurringGenericParameter().IsFalse();
        typeof(Demo<>).GetGenericArguments()[0].IsCuriouslyRecurringGenericParameter().IsTrue();
    }

    private class Demo<T> where T : Demo<T>
    {
    }
}