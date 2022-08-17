using System;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.MessagePack.Tests;

public class TestBase
{
    protected ISerializer<ReadOnlyMemory<byte>> GetSerializer() => new ServiceContainer()
        .AddRuntime(GetType().Assembly)
        .AddMessagePackSerializer()
        .AddTime().WithManagedTime().SetDefault()
        .AddLogging()
        .BuildServiceProvider()
        .UseLogging(x => x.UseInMemory())
        .ResolveSerializer<ReadOnlyMemory<byte>>(Abstractions.Constants.DefaultKey, Constants.MediaType);
}