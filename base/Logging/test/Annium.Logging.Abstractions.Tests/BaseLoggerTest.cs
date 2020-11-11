using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;

namespace Annium.Logging.Abstractions.Tests
{
    public class BaseLoggerTest
    {
        private readonly Func<Instant> _getInstant =
            () => Instant.FromUnixTimeSeconds(60);

        private readonly IList<LogMessage> _messages = new List<LogMessage>();

        [Fact]
        public void Log_ValidLevel_WritesLogEntry()
        {
            // arrange
            var logger = GetLogger();

            // act
            logger.Log(LogLevel.Trace, "sample");

            // assert
            _messages.Has(1);
            _messages.At(0).Instant.IsEqual(_getInstant());
            _messages.At(0).Level.IsEqual(LogLevel.Trace);
            _messages.At(0).Source.IsEqual(typeof(BaseLoggerTest));
            _messages.At(0).Message.IsEqual("sample");
        }

        [Fact]
        public void Log_InvalidLevel_OmitsLogEntry()
        {
            // arrange
            var logger = GetLogger(LogLevel.Debug);

            // act
            logger.Log(LogLevel.Trace, "sample");

            // assert
            _messages.IsEmpty();
        }

        [Fact]
        public void LogTrace_WritesTraceLogEntry()
        {
            // arrange
            var logger = GetLogger();

            // act
            logger.Trace("sample");

            // assert
            _messages.At(0).Level.IsEqual(LogLevel.Trace);
        }

        [Fact]
        public void LogDebug_WritesDebugLogEntry()
        {
            // arrange
            var logger = GetLogger();

            // act
            logger.Debug("sample");

            // assert
            _messages.At(0).Level.IsEqual(LogLevel.Debug);
        }

        [Fact]
        public void LogInfo_WritesInfoLogEntry()
        {
            // arrange
            var logger = GetLogger();

            // act
            logger.Info("sample");

            // assert
            _messages.At(0).Level.IsEqual(LogLevel.Info);
        }

        [Fact]
        public void LogWarn_WritesWarnLogEntry()
        {
            // arrange
            var logger = GetLogger();

            // act
            logger.Warn("sample");

            // assert
            _messages.At(0).Level.IsEqual(LogLevel.Warn);
        }

        [Fact]
        public void LogErrorException_WritesErrorExceptionLogEntry()
        {
            // arrange
            var logger = GetLogger();
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
            var logger = GetLogger();

            // act
            logger.Error("sample");

            // assert
            _messages.At(0).Level.IsEqual(LogLevel.Error);
            _messages.At(0).Message.IsEqual("sample");
        }

        private ILogger GetLogger(LogLevel minLogLevel = LogLevel.Trace)
        {
            var services = new ServiceCollection();

            services.AddSingleton<Func<Instant>>(() => Instant.FromUnixTimeSeconds(60));

            services.AddLogging(route => route
                .For(m => m.Level >= minLogLevel)
                .Use(ServiceDescriptor.Singleton(new LogHandler(_messages)))
            );

            return services.BuildServiceProvider().GetRequiredService<ILogger<BaseLoggerTest>>();
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