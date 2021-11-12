using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Data.Models.Extensions;
using Xunit;

namespace Annium.Core.Mapper.Tests.Resolvers
{
    public class DictionaryConstructorMapResolverTest
    {
        [Fact]
        public void ConstructorMapping_Works()
        {
            // arrange
            var mapper = GetMapper();
            var serialized = JsonSerializer.Serialize(new { Name = "Alex", Age = 20 });
            var value = new Dictionary<string, object> { { "Serialized", serialized } };

            // act
            var result = mapper.Map<C>(value);

            // assert
            result.IsShallowEqual(new C(0, 0, "Alex", 20));
        }

        private IMapper GetMapper() => new ServiceContainer()
            .AddRuntimeTools(Assembly.GetCallingAssembly(), false)
            .AddMapper(autoload: false)
            .AddProfile(ConfigureProfile)
            .BuildServiceProvider()
            .Resolve<IMapper>();

        private void ConfigureProfile(Profile p)
        {
            p.Map<Dictionary<string, object>, C>()
                .Ignore(x => new { x.IgnoredA, x.IgnoredB })
                .For(
                    x => new { x.Name, x.Age },
                    x => JsonSerializer.Deserialize<Info>(
                        x["Serialized"].ToString()!,
                        default(JsonSerializerOptions)
                    )
                );
        }

        private class A
        {
            public string Serialized { get; set; } = string.Empty;
        }

        private class C
        {
            public int IgnoredA { get; }
            public long IgnoredB { get; }
            public string Name { get; }
            public int Age { get; }

            public C(int ignoredA, long ignoredB, string name, int age)
            {
                IgnoredA = ignoredA;
                IgnoredB = ignoredB;
                Name = name;
                Age = age;
            }
        }

        private class Info
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
        }
    }
}