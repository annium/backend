using System.Threading.Tasks;
using Annium.Debug;

// ReSharper disable once CheckNamespace
namespace System;

public static class WhenCompletedExtensions
{
    public static async Task WhenCompleted<TSource>(
        this IObservable<TSource> source
    )
    {
        var tcs = new TaskCompletionSource<object?>();
        source.TraceOld("subscribe");
        using var _ = source.Subscribe(delegate { }, () =>
        {
            source.TraceOld("set - start");
            tcs.SetResult(null);
            source.TraceOld("set - done");
        });
        source.TraceOld("wait");
        await tcs.Task;
        source.TraceOld("done");
    }
}