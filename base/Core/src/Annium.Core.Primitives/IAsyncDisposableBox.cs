using System;
using System.Threading.Tasks;

namespace Annium.Core.Primitives
{
    public interface IAsyncDisposableBox : IAsyncDisposable
    {
        IAsyncDisposableBox Add(IDisposable disposable);
        IAsyncDisposableBox Add(IAsyncDisposable disposable);
        IAsyncDisposableBox Add(Action dispose);
        IAsyncDisposableBox Add(Func<Task> dispose);
    }
}