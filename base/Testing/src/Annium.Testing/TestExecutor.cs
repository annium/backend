﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Annium.Testing.Elements;
using Annium.Testing.Executors;

// ReSharper disable AccessToDisposedClosure

namespace Annium.Testing;

public class TestExecutor : ILogSubject<TestExecutor>
{
    public ILogger<TestExecutor> Logger { get; }
    private readonly TestingConfiguration _cfg;
    private readonly IServiceProvider _provider;
    private readonly PipelineExecutor _executor;

    public TestExecutor(
        TestingConfiguration cfg,
        IServiceProvider provider,
        PipelineExecutor executor,
        ILogger<TestExecutor> logger
    )
    {
        _cfg = cfg;
        _provider = provider;
        _executor = executor;
        Logger = logger;
    }

    public async Task RunTestsAsync(IEnumerable<Test> tests, Action<Test, TestResult> handleResult)
    {
        this.Log().Debug("Start tests execution");

        var concurrency = Environment.ProcessorCount;

        using var semaphore = new Semaphore(concurrency, concurrency);
        await Task.WhenAll(tests.FilterMask(_cfg.Filter).Select(async test =>
        {
            try
            {
                semaphore.WaitOne();
                this.Log().Debug($"Run test {test.DisplayName}");

                await using var scope = _provider.CreateAsyncScope();
                var target = new Target(scope.ServiceProvider, test, new TestResult());

                await _executor.ExecuteAsync(target);

                this.Log().Debug($"Complete test {test.DisplayName}");
                handleResult(target.Test, target.Result);
            }
            finally
            {
                semaphore.Release();
            }
        }));

        this.Log().Debug("Complete tests execution");
    }
}