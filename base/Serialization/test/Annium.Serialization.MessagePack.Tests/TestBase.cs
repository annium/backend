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
            .Resolve<IIndex<string, ISerializer<ReadOnlyMemory<byte>>>>()
            [Constants.Key];
    }
}