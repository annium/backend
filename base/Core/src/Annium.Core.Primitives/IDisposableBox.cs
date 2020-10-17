using System;
using System.Threading.Tasks;

namespace Annium.Core.Primitives
{
    public interface IDisposableBox : IAsyncDisposable
    {
        IDisposableBox Add(IDisposable disposable);
        IDisposableBox Add(IAsyncDisposable disposable);
        IDisposableBox Add(Action dispose);
        IDisposableBox Add(Func<Task> dispose);
    }
}