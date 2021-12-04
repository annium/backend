using System;
using Annium.Infrastructure.WebSockets.Domain;
using Annium.Net.WebSockets;
using NodaTime;

namespace Annium.Infrastructure.WebSockets.Client;

public interface IClientConfiguration : IClientConfigurationBase
{
    Uri Uri { get; }
    bool AutoConnect { get; }
    ClientWebSocketOptions WebSocketOptions { get; }
}

public interface ITestClientConfiguration : IClientConfigurationBase
{
    WebSocketOptions WebSocketOptions { get; }
}

public interface IClientConfigurationBase
{
    SerializationFormat Format { get; }
    Duration ResponseTimeout { get; }
}