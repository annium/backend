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
        source.Trace("subscribe");
        using var _ = source.Subscribe(delegate { }, () =>
        {
            source.Trace("set - start");
            tcs.SetResult(null);
            source.Trace("set - done");
        });
        source.Trace("wait");
        await tcs.Task;
        source.Trace("done");
    }
}