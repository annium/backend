using System;
using System.Threading.Tasks;
using Annium.Core.Primitives.Internal;

namespace Annium.Core.Primitives
{
    public static class Disposable
    {
        public static DisposableBox Box() => new();
        public static AsyncDisposableBox AsyncBox() => new();
        public static IAsyncDisposable Create(Func<Task> handle) => new AsyncDisposer(handle);
        public static IDisposable Create(Action handle) => new Disposer(handle);
    }
}