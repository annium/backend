using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using NodaTime;

namespace System
{
    public static class ObservableCustomOperatorExtensions
    {
        public static IObservable<TSource> ThrottleBy<TSource, TKey>(
            this IObservable<TSource> source,
            Func<TSource, TKey> getKey,
            Duration interval
        )
            where TKey : notnull
        {
            var clock = SystemClock.Instance;
            var intervalMilliseconds = (long) interval.TotalMilliseconds;

            return Observable.Create<TSource>(observer =>
            {
                var keys = new Dictionary<TKey, long>();

                return source.Subscribe(x =>
                {
                    var now = clock.GetCurrentInstant().ToUnixTimeMilliseconds();
                    var key = getKey(x);

                    var send = false;
                    lock (keys)
                    {
                        if (!keys.TryGetValue(key, out var till) || till <= now)
                        {
                            keys[key] = now + intervalMilliseconds;
                            send = true;
                        }
                    }

                    if (send)
                        observer.OnNext(x);
                }, observer.OnError, observer.OnCompleted);
            });
        }

        public static IObservable<Unit> Unit<TSource>(
            this IObservable<TSource> source
        ) => source.Select(_ => Reactive.Unit.Default);
    }
}