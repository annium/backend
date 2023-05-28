using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

public class SelectParallelAsyncTest
{
    [Fact]
    public async Task SelectParallelAsync_WorksCorrectly()
    {
        // arrange
        var log = new List<string>();
        var tcs = new TaskCompletionSource();
        using var observable = Observable.Range(1, 5)
            .SelectParallelAsync(async x =>
            {
                lock (log)
                    log.Add($"start: {x}");
                await Task.Delay(10);
                lock (log)
                    log.Add($"end: {x}");
                return x;
            })
            .Subscribe(x =>
            {
                lock (log)
                    log.Add($"sub: {x}");
            }, tcs.SetResult);

        await tcs.Task;

        log.Has(15);
        var starts = log.Select((x, i) => (x, i)).Where(x => x.x.StartsWith("start")).Select(x => x.i).ToArray();
        var ends = log.Select((x, i) => (x, i)).Where(x => !x.x.StartsWith("start")).Select(x => x.i).ToArray();
        // ends/subs go after all starts
        ends.All(x => starts.All(s => s < x)).IsTrue();
    }
}