using System;
using Annium.Debug;

namespace Annium.Extensions.Reactive.Internal;

internal static class ObserverObservableEventExtensions
{
    public static void Handle<TSource>(this IObserver<TSource> observer, ObservableEvent<TSource> e)
    {
        if (e.Error is not null)
        {
            observer.TraceOld($"error: {e.Error}");
            observer.OnError(e.Error);
            return;
        }

        if (e.IsCompleted)
        {
            observer.TraceOld("completed");
            observer.OnCompleted();
            return;
        }

        observer.TraceOld($"next: {e.Data}");
        observer.OnNext(e.Data);
    }
}