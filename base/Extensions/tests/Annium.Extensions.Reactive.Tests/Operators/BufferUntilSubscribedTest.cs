using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Extensions.Reactive.Tests.Operators;

public class BufferUntilSubscribedTest : TestBase
{
    public BufferUntilSubscribedTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task BufferUntilSubscribed_LightImmediateWorks()
    {
        // arrange
        var logger = Get<ILogger>();
        var (log, writeLog) = GetLog();

        var cts = new CancellationTokenSource();
        var observable = ObservableExt.StaticSyncInstance<string>(async ctx =>
        {
            var i = 0;
            while (i < 10)
            {
                var val = i.ToString();
                WriteLine($"Next: {val}");
                ctx.OnNext(val);

                await Task.Delay(10, CancellationToken.None);

                if (val == "2")
                {
                    WriteLine($"Error: {val}");
                    ctx.OnError(new Exception(val));
                }

                i++;
            }

            return () => Task.CompletedTask;
        }, cts.Token, logger).BufferUntilSubscribed(logger);

        // act
        await Capture(observable, writeLog);

        log.IsEqual(new[] { "d: 0", "d: 1", "d: 2", "e: 2", "c" });
    }

    [Fact]
    public async Task BufferUntilSubscribed_LightDelayedWorks()
    {
        // arrange
        var logger = Get<ILogger>();
        var (log, writeLog) = GetLog();

        var cts = new CancellationTokenSource();
        var observable = ObservableExt.StaticSyncInstance<string>(async ctx =>
        {
            var i = 0;
            while (i < 10)
            {
                var val = i.ToString();
                WriteLine($"Next: {val}");
                ctx.OnNext(val);

                await Task.Delay(10, CancellationToken.None);

                if (val == "2")
                {
                    WriteLine($"Error: {val}");
                    ctx.OnError(new Exception(val));
                }

                i++;
            }

            return () => Task.CompletedTask;
        }, cts.Token, logger).BufferUntilSubscribed(logger);

        // act
        await Task.Delay(100, CancellationToken.None);
        await Capture(observable, writeLog);

        log.IsEqual(new[] { "d: 0", "d: 1", "d: 2", "e: 2", "c" });
    }

    [Fact]
    public async Task BufferUntilSubscribed_HeavyImmediateWorks()
    {
        // arrange
        var logger = Get<ILogger>();
        var (log, writeLog) = GetLog();

        var cts = new CancellationTokenSource();
        var observable = ObservableExt.StaticSyncInstance<string>(async ctx =>
        {
            var i = 0;
            while (i < 10)
            {
                var val = i.ToString();
                WriteLine($"Next: {val}");
                ctx.OnNext(val);

                await Task.Delay(1, CancellationToken.None);

                if (val == "2")
                {
                    WriteLine($"Error: {val}");
                    ctx.OnError(new Exception(val));
                }

                i++;
            }

            return () => Task.CompletedTask;
        }, cts.Token, logger).BufferUntilSubscribed(logger);

        // act
        await Capture(observable, writeLog);

        log.IsEqual(new[] { "d: 0", "d: 1", "d: 2", "e: 2", "c" });
    }

    [Fact]
    public async Task BufferUntilSubscribed_HeavyDelayedWorks()
    {
        // arrange
        var logger = Get<ILogger>();
        var (log, writeLog) = GetLog();

        var cts = new CancellationTokenSource();
        var observable = ObservableExt.StaticSyncInstance<string>(async ctx =>
        {
            var i = 0;
            while (i < 10)
            {
                var val = i.ToString();
                WriteLine($"Next: {val}");
                ctx.OnNext(val);

                await Task.Delay(1, CancellationToken.None);

                if (val == "2")
                {
                    WriteLine($"Error: {val}");
                    ctx.OnError(new Exception(val));
                }

                i++;
            }

            return () => Task.CompletedTask;
        }, cts.Token, logger).BufferUntilSubscribed(logger);

        // act
        await Task.Delay(20, CancellationToken.None);
        await Capture(observable, writeLog);

        log.IsEqual(new[] { "d: 0", "d: 1", "d: 2", "e: 2", "c" });
    }

    [Fact]
    public async Task BufferUntilSubscribed_SyncImmediateWorks()
    {
        // arrange
        var logger = Get<ILogger>();
        var (log, writeLog) = GetLog();

        var cts = new CancellationTokenSource();
        var observable = ObservableExt.StaticAsyncInstance<string>(ctx =>
        {
            var i = 0;
            while (i < 10)
            {
                var val = i.ToString();
                WriteLine($"Next: {val}");
                ctx.OnNext(val);

                if (val == "2")
                {
                    WriteLine($"Error: {val}");
                    ctx.OnError(new Exception(val));
                }

                i++;
            }

            return Task.FromResult<Func<Task>>(() => Task.CompletedTask);
        }, cts.Token, logger).BufferUntilSubscribed(logger);

        // act
        await Capture(observable, writeLog);

        log.IsEqual(new[] { "d: 0", "d: 1", "d: 2", "e: 2", "c" });
    }

    [Fact]
    public async Task BufferUntilSubscribed_SyncDelayedWorks()
    {
        // arrange
        var logger = Get<ILogger>();
        var (log, writeLog) = GetLog();

        var cts = new CancellationTokenSource();
        var observable = ObservableExt.StaticSyncInstance<string>(ctx =>
        {
            var i = 0;
            while (i < 10)
            {
                var val = i.ToString();
                WriteLine($"Next: {val}");
                ctx.OnNext(val);

                if (val == "2")
                {
                    WriteLine($"Error: {val}");
                    ctx.OnError(new Exception(val));
                }

                i++;
            }

            return Task.FromResult<Func<Task>>(() => Task.CompletedTask);
        }, cts.Token, logger).BufferUntilSubscribed(logger);

        // act
        await Task.Delay(10, CancellationToken.None);
        await Capture(observable, writeLog);

        log.IsEqual(new[] { "d: 0", "d: 1", "d: 2", "e: 2", "c" });
    }

    private Task Capture(IObservable<string> observable, Action<string> log) => observable
        .Catch((Exception e) =>
        {
            log($"e: {e.Message}");
            return Observable.Empty<string>();
        })
        .Retry()
        .Do(
            x => log($"d: {x}"),
            () => log("c")
        )
        .WhenCompleted(Get<ILogger>());

    private (List<string>, Action<string>) GetLog()
    {
        // arrange
        var log = new List<string>();

        return (log, value =>
        {
            Console.WriteLine($"log: {value}");
            log.Add(value);
        });
    }

    private static void WriteLine(string message) => Console.WriteLine(message);
}