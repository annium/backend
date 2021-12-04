using System;
using Annium.Core.Internal;

namespace Annium.Extensions.Reactive.Internal;

internal static class ObserverObservableEventExtensions
{
    public static void Handle<TSource>(this IObserver<TSource> observer, ObservableEvent<TSource> e)
    {
        if (e.Error is not null)
        {
            observer.Trace($"error: {e.Error}");
            observer.OnError(e.Error);
            return;
        }

        if (e.IsCompleted)
        {
            observer.Trace("completed");
            observer.OnCompleted();
            return;
        }

        observer.Trace($"next: {e.Data}");
        observer.OnNext(e.Data);
    }
}