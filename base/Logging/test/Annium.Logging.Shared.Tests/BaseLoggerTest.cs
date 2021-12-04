using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Shared.Tests;

public class BaseLoggerTest
{
    private readonly IList<LogMessage<Context>> _messages = new List<LogMessage<Context>>();

    [Fact]
    public void Log_ValidLevel_WritesLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.Resolve<ILogSubject<BaseLoggerTest>>();
        var timeProvider = provider.Resolve<ITimeProvider>();

        // act
        subject.Log().Trace("sample");

        // assert
        _messages.Has(1);
        _messages.At(0).Instant.IsEqual(timeProvider.Now);
        _messages.At(0).Level.IsEqual(LogLevel.Trace);
        _messages.At(0).Source.IsEqual(typeof(BaseLoggerTest).FriendlyName());
        _messages.At(0).Message.IsEqual("sample");
    }

    [Fact]
    public void Log_InvalidLevel_OmitsLogEntry()
    {
        // arrange
        var provider = GetProvider(LogLevel.Debug);
        var subject = provider.Resolve<ILogSubject<BaseLoggerTest>>();

        // act
        subject.Log().Trace("sample");

        // assert
        _messages.IsEmpty();
    }

    [Fact]
    public void LogTrace_WritesTraceLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.Resolve<ILogSubject<BaseLoggerTest>>();

        // act
        subject.Log().Trace("sample");

        // assert
        _messages.At(0).Level.IsEqual(LogLevel.Trace);
    }

    [Fact]
    public void LogDebug_WritesDebugLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.Resolve<ILogSubject<BaseLoggerTest>>();

        // act
        subject.Log().Debug("sample");

        // assert
        _messages.At(0).Level.IsEqual(LogLevel.Debug);
    }

    [Fact]
    public void LogInfo_WritesInfoLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.Resolve<ILogSubject<BaseLoggerTest>>();

        // act
        subject.Log().Info("sample");

        // assert
        _messages.At(0).Level.IsEqual(LogLevel.Info);
    }

    [Fact]
    public void LogWarn_WritesWarnLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.Resolve<ILogSubject<BaseLoggerTest>>();

        // act
        subject.Log().Warn("sample");

        // assert
        _messages.At(0).Level.IsEqual(LogLevel.Warn);
    }

    [Fact]
    public void LogErrorException_WritesErrorExceptionLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.Resolve<ILogSubject<BaseLoggerTest>>();
        var exception = new Exception("sample");

        // act
        subject.Log().Error(exception);

        // assert
        _messages.At(0).Level.IsEqual(LogLevel.Error);
        _messages.At(0).Message.IsEqual(exception.Message);
        _messages.At(0).Exception.IsEqual(exception);
    }

    [Fact]
    public void LogErrorMessage_WritesErrorMessageLogEntry()
    {
        // arrange
        var provider = GetProvider();
        var subject = provider.Resolve<ILogSubject<BaseLoggerTest>>();

        // act
        subject.Log().Error("sample");

        // assert
        _messages.At(0).Level.IsEqual(LogLevel.Error);
        _messages.At(0).Message.IsEqual("sample");
    }

    private IServiceProvider GetProvider(LogLevel minLogLevel = LogLevel.Trace)
    {
        var container = new ServiceContainer();

        container.AddTime().WithManagedTime().SetDefault();

        container.AddLogging<Context>(route => route
            .For(m => m.Level >= minLogLevel)
            .UseInstance(new LogHandler(_messages), new LogRouteConfiguration())
        );

        return container.BuildServiceProvider();
    }

    private class LogHandler : ILogHandler<Context>
    {
        public IList<LogMessage<Context>> Messages { get; }

        public LogHandler(
            IList<LogMessage<Context>> messages
        )
        {
            Messages = messages;
        }

        public void Handle(LogMessage<Context> message)
        {
            Messages.Add(message);
        }
    }

    private class Context : ILogContext
    {
    }
}