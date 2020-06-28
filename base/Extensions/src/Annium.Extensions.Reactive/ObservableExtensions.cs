using System.Threading.Tasks;

namespace System
{
    public static class ObservableExtensions
    {
        public static IDisposable Subscribe<T>(this IObservable<T> source, Func<T, Task> onNext)
            => source.Subscribe(x => onNext(x).GetAwaiter().GetResult());
    }
}