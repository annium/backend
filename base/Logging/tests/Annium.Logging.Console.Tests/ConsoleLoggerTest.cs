using System;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Logging.Console.Tests;

public class ConsoleLoggerTest : TestBase, ILogSubject<ConsoleLoggerTest>
{
    public ILogger<ConsoleLoggerTest> Logger { get; }

    public ConsoleLoggerTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Logger = Get<ILogger<ConsoleLoggerTest>>();
    }

    [Fact]
    public void LogMessage_WritesLogMessageToConsole()
    {
        // arrange
        var subject = GetSubject();
        using var capture = ConsoleCapture.Start();

        // act
        subject.Log().Info("sample");

        // assert
        capture.Output.Contains("sample").IsTrue();
    }

    [Fact]
    public void LogAggregateException_WritesErrorsCountAndAllErrorsToConsole()
    {
        // arrange
        var subject = GetSubject();
        using var capture = ConsoleCapture.Start();

        // arrange
        var ex = new AggregateException(new Exception("xxx"), new Exception("yyy"));

        // act
        subject.Log().Error(ex);

        // assert
        capture.Output.Contains("2 error(s) in").IsTrue();
        capture.Output.Contains("xxx").IsTrue();
        capture.Output.Contains("yyy").IsTrue();
    }

    [Fact]
    public void LogException_WritesExceptionToConsole()
    {
        // arrange
        var subject = GetSubject();
        using var capture = ConsoleCapture.Start();

        // arrange
        var ex = new Exception("xxx");

        // act
        subject.Log().Error(ex);

        // assert
        capture.Output.Contains("xxx").IsTrue();
    }

    private ILogSubject<ConsoleLoggerTest> GetSubject(LogLevel minLogLevel = LogLevel.Trace)
    {
        var container = new ServiceContainer();

        container.AddTime().WithManagedTime().SetDefault();

        container.AddLogging();

        var provider = container.BuildServiceProvider();
        provider.UseLogging(
            route => route
                .For(m => m.Level >= minLogLevel)
                .UseConsole()
        );

        return provider.Resolve<ILogSubject<ConsoleLoggerTest>>();
    }
}