using System;

namespace Annium.Mesh.Server.Handlers;

public interface IHandlerBase<TAction>
    where TAction : struct, Enum
{
    static abstract ushort Version { get; }
    static abstract TAction Action { get; }
}
