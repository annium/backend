using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json.Tests.Converters;

await using var entry = Entrypoint.Default.Setup();

var (provider, _) = entry;

var serializer = provider.Resolve<ISerializer<string>>();

AbstractJsonConverterTest.KeyBase a = new AbstractJsonConverterTest.KeyChildA { Value = 1 };
AbstractJsonConverterTest.KeyBase b = new AbstractJsonConverterTest.KeyChildB { Value = 2 };
AbstractJsonConverterTest.KeyBaseContainer<AbstractJsonConverterTest.KeyBase> container =
    new AbstractJsonConverterTest.KeyDataContainer<AbstractJsonConverterTest.KeyBase> { Items = new[] { a, b } };
var str = serializer.Serialize(container);

// act
// ReSharper disable once UnusedVariable
var result = serializer.Deserialize<AbstractJsonConverterTest.KeyBaseContainer<AbstractJsonConverterTest.KeyBase>>(str);