using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Entrypoint;
using Annium.NodaTime.Serialization.Json;
using NodaTime;

await using var entry = Entrypoint.Default.Setup();

var convertersList = new[] { Converters.IsoDateIntervalConverter, Converters.LocalDateConverter };
var startLocalDate = new LocalDate(2012, 1, 2);
var endLocalDate = new LocalDate(2013, 6, 7);
var dateInterval = new DateInterval(startLocalDate, endLocalDate);

var testObject = new TestObject { Interval = dateInterval };

// ReSharper disable once UnusedVariable
var json = JsonSerializer.Serialize(testObject, With(convertersList));

JsonSerializerOptions With(params JsonConverter[] converters)
{
    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    foreach (var converter in converters)
        options.Converters.Add(converter);

    return options;
}

public class TestObject
{
    public DateInterval Interval { get; set; } = null!;
}