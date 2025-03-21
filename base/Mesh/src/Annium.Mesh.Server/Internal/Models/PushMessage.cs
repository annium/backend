using System;

namespace Annium.Mesh.Server.Internal.Models;

internal static class PushMessage
{
    public static PushMessage<T> New<T>(Guid connectionId, T message) => new(connectionId, message);
}

internal sealed record PushMessage<T>(Guid ConnectionId, T Message);
