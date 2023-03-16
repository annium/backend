using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Reflection.Tests.Types.Extensions;

public class ResolveGenericArgumentsByImplementationExtensionMainTests
{
    [Fact]
    public void TypeNull_Throws()
    {
        //assert
        Wrap.It(() => (null as Type)!.ResolveGenericArgumentsByImplementation(typeof(bool)))
            .Throws<ArgumentNullException>();
    }

    [Fact]
    public void TargetNull_Throws()
    {
        //assert
        Wrap.It(() => typeof(bool).ResolveGenericArgumentsByImplementation(null!))
            .Throws<ArgumentNullException>();
    }
}