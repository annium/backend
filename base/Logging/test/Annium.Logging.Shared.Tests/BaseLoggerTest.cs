using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Core.Runtime.Time;
using Annium.Logging.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Shared.Tests
{
    public class BaseLoggerTest
    {
        private readonly IList<LogMessage> _messages = new List<LogMessage>();

        [Fact]
        public void Log_ValidLevel_WritesLogEntry()
        {
            // arrange
            var provider = GetProvider();
            var logger = provider.Resolve<ILogger<BaseLoggerTest>>();
            var timeProvider = provider.Resolve<ITimeProvider>();

            // act
            logger.Log(LogLevel.Trace, "sample");

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
            var logger = provider.Resolve<ILogger<BaseLoggerTest>>();

            // act
            logger.Log(LogLevel.Trace, "sample");

            // assert
            _messages.IsEmpty();
        }

        [Fact]
        public void LogTrace_WritesTraceLogEntry()
        {
            // arrange
            var provider = GetProvider();
            var logger = provider.Resolve<ILogger<BaseLoggerTest>>();

            // act
            logger.Trace("sample");

            // assert
            _messages.At(0).Level.IsEqual(LogLevel.Trace);
        }

        [Fact]
        public void LogDebug_WritesDebugLogEntry()
        {
            // arrange
            var provider = GetProvider();
            var logger = provider.Resolve<ILogger<BaseLoggerTest>>();

            // act
            logger.Debug("sample");

            // assert
            _messages.At(0).Level.IsEqual(LogLevel.Debug);
        }

        [Fact]
        public void LogInfo_WritesInfoLogEntry()
        {
            // arrange
            var provider = GetProvider();
            var logger = provider.Resolve<ILogger<BaseLoggerTest>>();

            // act
            logger.Info("sample");

            // assert
            _messages.At(0).Level.IsEqual(LogLevel.Info);
        }

        [Fact]
        public void LogWarn_WritesWarnLogEntry()
        {
            // arrange
            var provider = GetProvider();
            var logger = provider.Resolve<ILogger<BaseLoggerTest>>();

            // act
            logger.Warn("sample");

            // assert
            _messages.At(0).Level.IsEqual(LogLevel.Warn);
        }

        [Fact]
        public void LogErrorException_WritesErrorExceptionLogEntry()
        {
            // arrange
            var provider = GetProvider();
            var logger = provider.Resolve<ILogger<BaseLoggerTest>>();
            var exception = new Exception("sample");

            // act
            logger.Error(exception);

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
            var logger = provider.Resolve<ILogger<BaseLoggerTest>>();

            // act
            logger.Error("sample");

            // assert
            _messages.At(0).Level.IsEqual(LogLevel.Error);
            _messages.At(0).Message.IsEqual("sample");
        }

        private IServiceProvider GetProvider(LogLevel minLogLevel = LogLevel.Trace)
        {
            var container = new ServiceContainer();

            container.AddTestTimeProvider();

            container.AddLogging(route => route
                .For(m => m.Level >= minLogLevel)
                .Use(ServiceDescriptor.Instance(new LogHandler(_messages), ServiceLifetime.Singleton))
            );

            return container.BuildServiceProvider();
        }
    }

    public class LogHandler : ILogHandler
    {
        public IList<LogMessage> Messages { get; }

        public LogHandler(
            IList<LogMessage> messages
        )
        {
            Messages = messages;
        }

        public void Handle(LogMessage message) => Messages.Add(message);
    }
}