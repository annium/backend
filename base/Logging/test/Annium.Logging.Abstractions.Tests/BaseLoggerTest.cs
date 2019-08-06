using System;
using System.Collections.Generic;
using Annium.Testing;
using NodaTime;

namespace Annium.Logging.Abstractions.Tests
{
    public class BaseLoggerTest
    {
        private readonly Func<Instant> getInstant =
            () => Instant.FromUnixTimeSeconds(60);

        private readonly IList<ValueTuple<Instant, LogLevel, string>> entries =
            new List<ValueTuple<Instant, LogLevel, string>>();

        [Fact]
        public void Log_ValidLevel_WritesLogEntry()
        {
            // arrange
            var logger = new Logger(entries, new LoggerConfiguration(LogLevel.Trace), getInstant);

            // act
            logger.Log(LogLevel.Trace, "sample");

            // assert
            entries.Has(1).At(0).IsEqual((getInstant(), LogLevel.Trace, "sample"));
        }

        [Fact]
        public void Log_InvalidLevel_OmitsLogEntry()
        {
            // arrange
            var logger = new Logger(entries, new LoggerConfiguration(LogLevel.Debug), getInstant);

            // act
            logger.Log(LogLevel.Trace, "sample");

            // assert
            entries.IsEmpty();
        }

        [Fact]
        public void LogTrace_WritesTraceLogEntry()
        {
            // arrange
            var logger = new Logger(entries, new LoggerConfiguration(LogLevel.Trace), getInstant);

            // act
            logger.Trace("sample");

            // assert
            entries.At(0).Item2.IsEqual(LogLevel.Trace);
        }

        [Fact]
        public void LogDebug_WritesDebugLogEntry()
        {
            // arrange
            var logger = new Logger(entries, new LoggerConfiguration(LogLevel.Debug), getInstant);

            // act
            logger.Debug("sample");

            // assert
            entries.At(0).Item2.IsEqual(LogLevel.Debug);
        }

        [Fact]
        public void LogInfo_WritesInfoLogEntry()
        {
            // arrange
            var logger = new Logger(entries, new LoggerConfiguration(LogLevel.Info), getInstant);

            // act
            logger.Info("sample");

            // assert
            entries.At(0).Item2.IsEqual(LogLevel.Info);
        }

        [Fact]
        public void LogWarn_WritesWarnLogEntry()
        {
            // arrange
            var logger = new Logger(entries, new LoggerConfiguration(LogLevel.Warn), getInstant);

            // act
            logger.Warn("sample");

            // assert
            entries.At(0).Item2.IsEqual(LogLevel.Warn);
        }

        [Fact]
        public void LogErrorException_WritesErrorExceptionLogEntry()
        {
            // arrange
            var logger = new Logger(entries, new LoggerConfiguration(LogLevel.Error), getInstant);

            // act
            logger.Error(new Exception("sample"));

            // assert
            entries.At(0).Item2.IsEqual(LogLevel.Error);
            entries.At(0).Item3.IsEqual("sample");
        }

        [Fact]
        public void LogErrorMessage_WritesErrorMessageLogEntry()
        {
            // arrange
            var logger = new Logger(entries, new LoggerConfiguration(LogLevel.Error), getInstant);

            // act
            logger.Error("sample");

            // assert
            entries.At(0).Item2.IsEqual(LogLevel.Error);
            entries.At(0).Item3.IsEqual("sample");
        }
    }

    [Fixture]
    public class Logger : BaseLogger<BaseLoggerTest>
    {
        public IList<ValueTuple<Instant, LogLevel, string>> Entries { get; }

        public Logger(
            IList<ValueTuple<Instant, LogLevel, string>> entries,
            LoggerConfiguration configuration,
            Func<Instant> getInstant
        ) : base(configuration, getInstant)
        {
            Entries = entries;
        }

        protected override void LogException(Instant instant, LogLevel level, Exception exception) =>
        Entries.Add((instant, level, exception.Message));

        protected override void LogMessage(Instant instant, LogLevel level, string message) =>
        Entries.Add((instant, level, message));
    }
}