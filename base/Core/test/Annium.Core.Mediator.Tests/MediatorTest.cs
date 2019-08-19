using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Annium.Logging.InMemory;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Annium.Core.Mediator.Tests
{
    public class MediatorTest
    {
        // single non-generic final handler works
        // single generic final handler works

        [Fact]
        public async Task SingleClosedHandler_Works()
        {
            // arrange
            var(mediator, logs) = GetMediator(typeof(ClosedFinalHandler));
            var request = new Base { Value = "base" };

            // act
            var response = await mediator.SendAsync<Base, One>(request);

            // assert
            response.GetHashCode().IsEqual(new One { First = request.Value.Length, Value = request.Value }.GetHashCode());
            logs.Has(2);
            logs.At(0).Item3.IsEqual(typeof(ClosedFinalHandler).FullName);
            logs.At(1).Item3.IsEqual(request.GetHashCode().ToString());
        }

        [Fact]
        public async Task SingleOpenHandler_WithExpectedParameters_Works()
        {
            // arrange
            var(mediator, logs) = GetMediator(typeof(OpenFinalHandler<,>));
            var request = new Two { Second = 2, Value = "one two three" };

            // act
            var response = await mediator.SendAsync<Two, Base>(request);

            // assert
            response.GetHashCode().IsEqual(new Base { Value = "one_two_three" }.GetHashCode());
            logs.Has(2);
            logs.At(0).Item3.IsEqual(typeof(OpenFinalHandler<Two, Base>).FullName);
            logs.At(1).Item3.IsEqual(request.GetHashCode().ToString());
        }

        [Fact]
        public async Task PerformanceTest()
        {
            // arrange
            var(mediator, logs) = GetMediator(typeof(OpenFinalHandler<,>));
            var request = new Two { Second = 2, Value = "one two three" };

            // act
            for (var i = 0; i < 1000000; i++)
                await mediator.SendAsync<Two, Base>(request);
        }

        private ValueTuple<IMediator, IReadOnlyList<ValueTuple<Instant, LogLevel, string>>> GetMediator(params Type[] handlerTypes)
        {
            var logger = new InMemoryLogger<MediatorTest>(
                new LoggerConfiguration(LogLevel.Trace),
                SystemClock.Instance.GetCurrentInstant
            );

            var provider = new ServiceCollection()
                .AddSingleton<ILogger<MediatorTest>>(logger)
                .AddMediatorConfiguration(cfg =>
                {
                    foreach (var handlerType in handlerTypes)
                        cfg.Add(handlerType);
                })
                .AddMediator()
                .BuildServiceProvider();

            return (provider.GetRequiredService<IMediator>(), logger.Logs);
        }

        private class OpenFinalHandler<TRequest, TResponse> : IFinalRequestHandler<TRequest, TResponse>
            where TRequest : TResponse
        where TResponse : Base, new()
        {
            private readonly ILogger<MediatorTest> logger;

            public OpenFinalHandler(
                ILogger<MediatorTest> logger
            )
            {
                this.logger = logger;
            }

            public Task<TResponse> HandleAsync(
                TRequest request,
                CancellationToken cancellationToken
            )
            {
                logger.Info(this.GetType().FullName);
                logger.Info(request.GetHashCode().ToString());

                var response = new TResponse() { Value = request.Value.Replace(' ', '_') };

                return Task.FromResult(response);
            }
        }

        private class ClosedFinalHandler : IFinalRequestHandler<Base, One>
        {
            private readonly ILogger<MediatorTest> logger;

            public ClosedFinalHandler(
                ILogger<MediatorTest> logger
            )
            {
                this.logger = logger;
            }

            public Task<One> HandleAsync(
                Base request,
                CancellationToken cancellationToken
            )
            {
                logger.Info(this.GetType().FullName);
                logger.Info(request.GetHashCode().ToString());

                return Task.FromResult(new One() { First = request.Value.Length, Value = request.Value });
            }
        }

        private class Authored<T>
        {
            public int AuthorId { get; set; }
            public T Entity { get; set; }

            public override int GetHashCode() => 13 * AuthorId.GetHashCode() + Entity.GetHashCode();
        }

        private class Base
        {
            public string Value { get; set; }

            public override int GetHashCode() => Value.GetHashCode();
        }

        private class One : Base
        {
            public long First { get; set; }

            public override int GetHashCode() => 7 * base.GetHashCode() + First.GetHashCode();
        }

        private class Two : Base
        {
            public int Second { get; set; }

            public override int GetHashCode() => 11 * base.GetHashCode() + Second.GetHashCode();
        }

        private interface IBase
        {
            string Value { get; set; }
        }
    }
}