using System;
using Annium.Net.WebSockets;
using NodaTime;

namespace Annium.Infrastructure.WebSockets.Client;

public interface IClientConfiguration : IClientConfigurationBase
{
    Uri Uri { get; }
    ClientWebSocketOptions WebSocketOptions { get; }
}

public interface ITestClientConfiguration : IClientConfigurationBase
{
    ServerWebSocketOptions WebSocketOptions { get; }
}

public interface IClientConfigurationBase
{
    Duration ResponseTimeout { get; }
}