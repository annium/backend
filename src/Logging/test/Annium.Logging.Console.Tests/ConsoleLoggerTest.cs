using System;
using Annium.Extensions.DependencyInjection;
using Annium.Logging.Abstractions;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Annium.Logging.Console.Tests
{
    public class ConsoleLoggerTest
    {
        private readonly Func<Instant> getInstant = () => Instant.FromUnixTimeSeconds(60);

        [Fact]
        public void LogMessage_WritesLogMessageToConsole()
        {
            // arrange
            var logger = GetLogger();

            using(var capture = ConsoleCapture.Start())
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

            using(var capture = ConsoleCapture.Start())
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

            using(var capture = ConsoleCapture.Start())
            {
                // arrange
                var ex = new Exception("xxx");

                // act
                logger.Error(ex);

                // assert
                capture.Output.Contains("xxx").IsTrue();
            }
        }

        private ILogger<ConsoleLoggerTest> GetLogger()
        {
            var services = new ServiceCollection();
            services.AddSingleton(getInstant);
            services.AddSingleton(new LoggerConfiguration(LogLevel.Trace));
            services.AddConsoleLogger();

            return services.BuildServiceProvider().GetRequiredService<ILogger<ConsoleLoggerTest>>();
        }
    }
}