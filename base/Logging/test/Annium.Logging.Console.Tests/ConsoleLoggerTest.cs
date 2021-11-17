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
            var subject = GetSubject();

            using (var capture = ConsoleCapture.Start())
            {
                // act
                subject.Log().Info("sample");

                // assert
                capture.Output.Contains("sample").IsTrue();
            }
        }

        [Fact]
        public void LogAggregateException_WritesErrorsCountAndAllErrorsToConsole()
        {
            // arrange
            var subject = GetSubject();

            using (var capture = ConsoleCapture.Start())
            {
                // arrange
                var ex = new AggregateException(new Exception("xxx"), new Exception("yyy"));

                // act
                subject.Log().Error(ex);

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
            var subject = GetSubject();

            using (var capture = ConsoleCapture.Start())
            {
                // arrange
                var ex = new Exception("xxx");

                // act
                subject.Log().Error(ex);

                // assert
                capture.Output.Contains("xxx").IsTrue();
            }
        }

        private ILogSubject GetSubject(LogLevel minLogLevel = LogLevel.Trace)
        {
            var container = new ServiceContainer();

            container.AddManagedTimeProvider();

            container.AddLogging(route => route
                .For(m => m.Level >= minLogLevel)
                .UseConsole());

            return container.BuildServiceProvider().Resolve<ILogSubject<ConsoleLoggerTest>>();
        }
    }
}