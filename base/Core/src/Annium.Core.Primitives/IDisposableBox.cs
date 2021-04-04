using System;
using System.Collections.Generic;

namespace Annium.Core.Primitives
{
    public interface IDisposableBox : IDisposable
    {
        IDisposableBox Add(IDisposable disposable);
        IDisposableBox AddRange(IEnumerable<IDisposable> disposables);
        IDisposableBox Add(Action dispose);
        IDisposableBox Add(IEnumerable<Action> disposes);
    }
}