using System;

namespace Annium.Core.Primitives
{
    public interface IDisposableBox : IDisposable
    {
        IDisposableBox Add(IDisposable disposable);
        IDisposableBox Add(Action dispose);
    }
}