using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.BinaryString.Tests
{
    public class TestBase
    {
        protected ISerializer<byte[], string> GetSerializer() => new ServiceContainer()
            .AddRuntimeTools(GetType().Assembly, false)
            .AddBinaryStringSerializer()
            .BuildServiceProvider()
            .ResolveSerializer<byte[], string>(Abstractions.Constants.DefaultKey, Constants.MediaType);
    }
}