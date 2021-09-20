using System.Threading.Tasks;

namespace System
{
    public static class WhenCompletedExtensions
    {
        public static async Task WhenCompleted<TSource>(
            this IObservable<TSource> source
        )
        {
            var tcs = new TaskCompletionSource<object?>();
            using var _ = source.Subscribe(delegate { }, () => tcs.SetResult(null));
            await tcs.Task;
        }
    }
}