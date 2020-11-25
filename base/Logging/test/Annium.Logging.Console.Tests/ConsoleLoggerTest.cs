using System;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Console.Tests
{
    public class ConsoleLoggerTest
    {
        [Fact]
        public void LogMessage_WritesLogMessageToConsole()
        {
            // arrange
            var logger = GetLogger();

            using (var capture = ConsoleCapture.Start())
            {
                // act
                logger.Info("sample");

                // assert
                capture.Output.Contains("sample").IsTrue();
            }
        }

        [Fact]
        public void LogAggregateException_WritesErrorsCountAndAllErrorsToConsole()
        {
            // arrange
            var logger = GetLogger();

            using (var capture = ConsoleCapture.Start())
            {
                // arrange
                var ex = new AggregateException(new Exception("xxx"), new Exception("yyy"));

                // act
                logger.Error(ex);

                // assert
                capture.Output.Contains("Errors (2):").IsTrue();
                capture.Output.Contains("xxx").IsTrue();
                capture.Output.Contains("yyy").IsTrue();
            }
        }

        [Fact]
        public void LogException_WritesExceptionToConsole()
        {
            // arrange
            var logger = GetLogger();

            using (var capture = ConsoleCapture.Start())
            {
                // arrange
                var ex = new Exception("xxx");

                // act
                logger.Error(ex);

                // assert
                capture.Output.Contains("xxx").IsTrue();
            }
        }

        private ILogger GetLogger(LogLevel minLogLevel = LogLevel.Trace)
        {
            var container = new ServiceContainer();

            container.AddTestTimeProvider();

            container.AddLogging(route => route
                .For(m => m.Level >= minLogLevel)
                .UseConsole());

            return container.BuildServiceProvider().Resolve<ILogger<ConsoleLoggerTest>>();
        }
    }
}