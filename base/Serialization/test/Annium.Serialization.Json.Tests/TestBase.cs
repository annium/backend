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
            container.AddJsonSerializers()
                .Configure(opts => opts.UseCamelCaseNamingPolicy())
                .SetDefault();

            return container.BuildServiceProvider()
                .ResolveSerializer<string>(Abstractions.Constants.DefaultKey, Constants.MediaType);
        }
    }
}