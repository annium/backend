using System;

namespace Annium.Core.Primitives.Internal;

internal class EmptyDisposer : IDisposable
{
    public void Dispose()
    {
    }
}