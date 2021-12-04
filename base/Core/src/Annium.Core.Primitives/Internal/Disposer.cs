using System;

namespace Annium.Core.Primitives.Internal;

internal class Disposer : IDisposable
{
    private readonly Action _handle;

    public Disposer(Action handle)
    {
        _handle = handle;
    }

    public void Dispose() => _handle();
}