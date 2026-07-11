using System;
using Annium.MessageBus.Nats.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Nats.Tests;

/// <summary>
/// Pure unit tests for <see cref="NatsConfigurationBuilder"/> URL parsing and validation (no broker).
/// </summary>
public sealed class NatsConfigurationBuilderTests
{
    /// <summary>
    /// A valid <c>nats://</c> URL is accepted and preserved.
    /// </summary>
    [Fact]
    public void Url_Nats_IsAccepted()
    {
        var builder = new NatsConfigurationBuilder();
        builder.Url("nats://localhost:4222");
        var config = builder.Build();
        config.Url.ToString().Is("nats://localhost:4222/");
    }

    /// <summary>
    /// A valid <c>tls://</c> URL is accepted.
    /// </summary>
    [Fact]
    public void Url_Tls_IsAccepted()
    {
        var builder = new NatsConfigurationBuilder();
        builder.Url("tls://broker:4222");
        var config = builder.Build();
        config.Url.Scheme.Is("tls");
    }

    /// <summary>
    /// A URL with an unsupported scheme is rejected.
    /// </summary>
    [Fact]
    public void Url_InvalidScheme_Throws()
    {
        Wrap.It(() => new NatsConfigurationBuilder().Url("http://localhost:4222")).Throws<ArgumentException>();
    }

    /// <summary>
    /// An empty URL is rejected.
    /// </summary>
    [Fact]
    public void Url_Empty_Throws()
    {
        Wrap.It(() => new NatsConfigurationBuilder().Url("  ")).Throws<ArgumentException>();
    }

    /// <summary>
    /// Building without a configured URL throws.
    /// </summary>
    [Fact]
    public void Build_WithoutUrl_Throws()
    {
        Wrap.It(() => new NatsConfigurationBuilder().Build()).Throws<InvalidOperationException>();
    }
}
