using System;
using System.Threading.Tasks;
using Annium.Core.Primitives.Internal;

namespace Annium.Core.Primitives
{
    public static class Disposable
    {
        public static IDisposableBox Box() => new DisposableBox();
        public static IAsyncDisposableBox AsyncBox() => new AsyncDisposableBox();
        public static IAsyncDisposable Create(Func<Task> handle) => new AsyncDisposer(handle);
        public static IDisposable Create(Action handle) => new Disposer(handle);
    }
}