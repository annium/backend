using System.Net.Mime;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Tests
{
    public class TestBase
    {
        protected ISerializer<string> GetSerializer() => new ServiceContainer()
            .AddRuntimeTools(GetType().Assembly, false)
            .AddJsonSerializers((sp, opts) => opts.ConfigureDefault(sp.Resolve<ITypeManager>()))
            .BuildServiceProvider()
            .Resolve<IIndex<string, ISerializer<string>>>()
            [MediaTypeNames.Application.Json];
    }
}