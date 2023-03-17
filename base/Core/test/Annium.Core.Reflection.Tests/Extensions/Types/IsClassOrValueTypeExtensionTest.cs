using System;
using System.IO;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Reflection.Tests.Extensions.Types;

public class IsConstructableExtensionTest
{
    [Fact]
    public void OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.IsConstructable()).Throws<ArgumentNullException>();
    }

    [Fact]
    public void Class_Works()
    {
        // assert
        typeof(object).IsConstructable().IsTrue();
        typeof(FileInfo).IsConstructable().IsTrue();
        typeof(Stream).IsConstructable().IsFalse();
    }

    [Fact]
    public void Struct_Works()
    {
        // assert
        typeof(long).IsConstructable().IsTrue();
        typeof(ValueTuple<>).IsConstructable().IsTrue();
    }
}