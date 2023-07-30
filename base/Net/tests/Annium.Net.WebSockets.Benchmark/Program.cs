using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

var config = new ManualConfig()
    .AddExporter(MarkdownExporter.Default)
    .AddDiagnoser(MemoryDiagnoser.Default)
    .AddJob(Job.Default
        .WithWarmupCount(0)
        .WithLaunchCount(1)
        .WithIterationCount(3)
        .WithStrategy(RunStrategy.Throughput)
        .WithPlatform(Platform.X64)
        .WithRuntime(CoreRuntime.Core70))
    .AddValidator(JitOptimizationsValidator.DontFailOnError)
    .AddLogger(ConsoleLogger.Default)
    .AddColumnProvider(DefaultColumnProviders.Instance);

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);