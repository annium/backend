using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Core.Primitives
{
    public interface IAsyncDisposableBox : IAsyncDisposable
    {
        IAsyncDisposableBox Add(IDisposable disposable);
        IAsyncDisposableBox Add(IEnumerable<IDisposable> disposables);
        IAsyncDisposableBox Add(IAsyncDisposable disposable);
        IAsyncDisposableBox Add(IEnumerable<IAsyncDisposable> disposables);
        IAsyncDisposableBox Add(Action dispose);
        IAsyncDisposableBox Add(IEnumerable<Action> disposes);
        IAsyncDisposableBox Add(Func<Task> dispose);
        IAsyncDisposableBox Add(IEnumerable<Func<Task>> disposes);
    }
}