using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests
{
    public class ObservableAsyncOperatorExtensionsTest
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

        [Fact]
        public async Task SelectSequentialAsync_WorksCorrectly()
        {
            // arrange
            var log = new List<string>();
            var tcs = new TaskCompletionSource();
            using var observable = Observable.Range(1, 5)
                .SelectSequentialAsync(async x =>
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
            var expectedLog = Enumerable.Range(1, 5)
                .Select(x => new[]
                {
                    $"start: {x}",
                    $"end: {x}",
                    $"sub: {x}"
                })
                .SelectMany(x => x)
                .ToArray();
            log.IsEqual(expectedLog);
        }

        [Fact]
        public async Task CatchAsync_WorksCorrectly()
        {
            // arrange
            var log = new List<string>();
            var tcs = new TaskCompletionSource();
            using var observable = Observable.Range(1, 5)
                .Select(x =>
                {
                    if (x == 3)
                        throw new InvalidOperationException("3");

                    lock (log)
                        log.Add($"add: {x}");

                    return x;
                })
                .CatchAsync(async (InvalidOperationException e) =>
                {
                    await Task.Delay(10);
                    lock (log)
                        log.Add($"err: {e.Message}");

                    return Observable.Empty<int>();
                })
                .Subscribe(x =>
                {
                    lock (log)
                        log.Add($"sub: {x}");
                }, () =>
                {
                    lock (log)
                        log.Add("done");
                    tcs.SetResult();
                });

            await tcs.Task;

            log.Has(6);
            log[4].Is("err: 3");
            log[5].Is("done");
        }
    }
}