using System;
using System.Threading.Tasks;
using Annium.Debug;
using Annium.Internal;

namespace Annium;

public static class Disposable
{
    public static DisposableBox Box(ITracer tracer) => new(tracer);
    public static AsyncDisposableBox AsyncBox(ITracer tracer) => new(tracer);
    public static IAsyncDisposable Create(Func<Task> handle) => new AsyncDisposer(handle);
    public static IDisposable Create(Action handle) => new Disposer(handle);
    public static readonly IDisposable Empty = new EmptyDisposer();
}