using System.Net.Mime;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Tests
{
    public class TestBase
    {
        protected ISerializer<string> GetSerializer()
        {
            var container = new ServiceContainer();
            container.AddRuntimeTools(GetType().Assembly, false);
            container.AddJsonSerializers().SetDefault();
            return container.BuildServiceProvider()
                .Resolve<IIndex<string, ISerializer<string>>>()
                [MediaTypeNames.Application.Json];
        }
    }
}