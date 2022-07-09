using System;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.MessagePack.Tests;

public class TestBase
{
    protected ISerializer<ReadOnlyMemory<byte>> GetSerializer() => new ServiceContainer()
        .AddMessagePackSerializer()
        .AddRuntime(GetType().Assembly)
        .BuildServiceProvider()
        .ResolveSerializer<ReadOnlyMemory<byte>>(Abstractions.Constants.DefaultKey, Constants.MediaType);
}