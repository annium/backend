using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Tests
{
    public class TestBase
    {
        protected ISerializer<string> GetSerializer()
        {
            var container = new ServiceContainer();
            container.AddRuntimeTools(GetType().Assembly, false, GetType().Assembly.ShortName(), typeof(IEnumerable<>).Assembly.ShortName());
            container.AddJsonSerializers()
                .Configure(opts => opts.UseCamelCaseNamingPolicy())
                .SetDefault();

            return container.BuildServiceProvider()
                .ResolveSerializer<string>(Abstractions.Constants.DefaultKey, Constants.MediaType);
        }
    }
}