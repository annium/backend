using System;
using System.Linq;
using Annium.MessageBus.Kafka.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Kafka.Tests;

/// <summary>
/// Unit tests for <see cref="BootstrapServersParser"/> (parse/validate/format). Pure (no broker).
/// </summary>
public class BootstrapServersParserTests
{
    /// <summary>
    /// A single valid entry parses to one endpoint.
    /// </summary>
    [Fact]
    public void Parse_SingleEntry_ParsesHostAndPort()
    {
        var endpoints = BootstrapServersParser.Parse("localhost:9092");

        endpoints.Has(1);
        endpoints.At(0).Is(new KafkaEndpoint("localhost", 9092));
    }

    /// <summary>
    /// A list of entries parses in order, stripping schemes/paths, trimming, and dropping empties.
    /// </summary>
    [Fact]
    public void Parse_MultipleEntries_StripsAndTrims()
    {
        var endpoints = BootstrapServersParser.Parse("PLAINTEXT://a:1/, b:2 ,,[::1]:9092");

        endpoints.Select(e => e.ToString()).ToArray().SequenceEqual(["a:1", "b:2", "[::1]:9092"]).Is(true);
    }

    /// <summary>
    /// Invalid inputs throw <see cref="ArgumentException"/>.
    /// </summary>
    /// <param name="input">The invalid input.</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("localhost")]
    [InlineData("localhost:")]
    [InlineData(":9092")]
    [InlineData("localhost:0")]
    [InlineData("localhost:70000")]
    [InlineData("localhost:abc")]
    [InlineData("localhost:-1")]
    [InlineData("bad host:9092")]
    public void Parse_Invalid_Throws(string input)
    {
        Wrap.It(() => BootstrapServersParser.Parse(input)).Throws<ArgumentException>();
    }

    /// <summary>
    /// Format joins endpoints into a canonical comma-separated host:port list.
    /// </summary>
    [Fact]
    public void Format_JoinsEndpoints()
    {
        var text = BootstrapServersParser.Format([new KafkaEndpoint("a", 1), new KafkaEndpoint("b", 2)]);

        text.Is("a:1,b:2");
    }

    /// <summary>
    /// Format is the inverse of Parse up to normalization (round-trip).
    /// </summary>
    [Fact]
    public void ParseThenFormat_Normalizes()
    {
        BootstrapServersParser
            .Format(BootstrapServersParser.Parse("PLAINTEXT://127.0.0.1:50286/ , h:2"))
            .Is("127.0.0.1:50286,h:2");
    }
}
