using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NodaTime;

namespace System
{
    public static class ObservableExtensions
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

        public static IDisposable Subscribe<T>(this IObservable<T> source, Func<T, Task> onNext)
            => source.Subscribe(x => onNext(x).GetAwaiter().GetResult());
    }
}