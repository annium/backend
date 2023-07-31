using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Annium.Net.WebSockets.Benchmark;
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

var startup = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
var serverFileName = OperatingSystem.IsWindows() ? "Annium.Net.WebSockets.WorkloadServer.exe" : "Annium.Net.WebSockets.WorkloadServer";
var serverFilePath = Path.Combine(startup, serverFileName);
var serverProcess = Process.Start(serverFilePath, $"{Constants.Port} {Constants.TotalMessages}");

Thread.Sleep(300);

var config = new ManualConfig()
    .AddExporter(MarkdownExporter.Default)
    .AddDiagnoser(MemoryDiagnoser.Default)
    .AddJob(Job.Default
        .WithWarmupCount(2)
        .WithLaunchCount(3)
        .WithIterationCount(5)
        .WithStrategy(RunStrategy.Throughput)
        .WithPlatform(Platform.X64)
        .WithRuntime(CoreRuntime.Core70))
    .AddValidator(JitOptimizationsValidator.DontFailOnError)
    .AddLogger(ConsoleLogger.Default)
    .AddColumnProvider(DefaultColumnProviders.Instance);

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);

serverProcess.Kill();