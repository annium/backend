using System;
using System.Collections.Generic;
using Annium.Core.Runtime.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Runtime.Tests.Types;

public class TypeIdTest
{
    [Fact]
    public void PlainId_Works()
    {
        Assert(typeof(int));
    }

    [Fact]
    public void ConstructedGenericId_Works()
    {
        Assert(typeof(Dictionary<string, List<int>>));
    }

    [Fact]
    public void OpenGenericId_Works()
    {
        Assert(typeof(Dictionary<,>));
    }

    private void Assert(Type type)
    {
        // act
        var id = type.GetId();

        // assert
        id.Id.Contains(type.Namespace!).IsTrue();
        id.Id.Contains(type.Name).IsTrue();
        (id == type.GetId()).IsTrue();
        var tm = GetTypeManager();
        var parsed = TypeId.TryParse(id.Id, tm);
        (parsed == id).IsTrue();
        parsed!.Type.Is(type);
    }

    private ITypeManager GetTypeManager() => TypeManager.GetInstance(GetType().Assembly, false);
}