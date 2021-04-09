using System;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.MessagePack.Tests
{
    public class TestBase
    {
        protected ISerializer<ReadOnlyMemory<byte>> GetSerializer() => new ServiceContainer()
            .AddMessagePackSerializer()
            .AddRuntimeTools(GetType().Assembly, false)
            .BuildServiceProvider()
            .ResolveSerializer<ReadOnlyMemory<byte>>(Abstractions.Constants.DefaultKey, Constants.MediaType);
    }
}