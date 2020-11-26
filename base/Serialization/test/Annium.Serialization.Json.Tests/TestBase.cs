using System.Net.Mime;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Tests
{
    public class TestBase
    {
        protected ISerializer<string> GetSerializer() => new ServiceContainer()
            .AddRuntimeTools(GetType().Assembly, false)
            .AddJsonSerializers()
            .BuildServiceProvider()
            .Resolve<IIndex<string, ISerializer<string>>>()
            [MediaTypeNames.Application.Json];
    }
}