using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations;
using Annium.Logging.Abstractions;
using Annium.Logging.InMemory;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Annium.Core.Mediator.Tests
{
    public class MediatorTest
    {
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
            logs.At(0).Message.IsEqual(typeof(ClosedFinalHandler).FullName);
            logs.At(1).Message.IsEqual(request.GetHashCode().ToString());
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
            logs.At(0).Message.IsEqual(typeof(OpenFinalHandler<Two, Base>).FullName);
            logs.At(1).Message.IsEqual(request.GetHashCode().ToString());
        }

        [Fact]
        public async Task ChainOfHandlers_WithExpectedParameters_Works()
        {
            // arrange
            var(mediator, logs) = GetMediator(typeof(ConversionHandler<,>), typeof(ValidationHandler<,>), typeof(OpenFinalHandler<,>));
            var request = new Two { Second = 2, Value = "one two three" };
            var payload = new Request<Two>(request);

            // act
            var response = (await mediator.SendAsync<Request<Two>, Response<IBooleanResult<Base>>>(payload)).Value;

            // assert
            response.IsSuccess.IsTrue();
            response.Data.GetHashCode().IsEqual(new Base { Value = "one_two_three" }.GetHashCode());
            logs.Has(6);
            logs.At(0).Message.IsEqual($"Deserialize Request to {typeof(Two).Name}");
            logs.At(1).Message.IsEqual($"Start {typeof(Two).Name} validation");
            logs.At(2).Message.IsEqual($"Status of {typeof(Two).Name} validation: {true}");
            logs.At(3).Message.IsEqual(typeof(OpenFinalHandler<Two, Base>).FullName);
            logs.At(4).Message.IsEqual(request.GetHashCode().ToString());
            logs.At(5).Message.IsEqual($"Serialize {typeof(IBooleanResult<Base>).Name} to Response");
        }

        private ValueTuple<IMediator, IReadOnlyList<LogMessage>> GetMediator(params Type[] handlerTypes)
        {
            var logHandler = new InMemoryLogHandler();

            var provider = new ServiceCollection()
                .AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant)
                .AddLogging(route => route.For(m => m.Source == typeof(MediatorTest)).UseInMemory(logHandler))
                .AddSingleton<Func<One, bool>>(value => value.First % 2 == 1)
                .AddSingleton<Func<Two, bool>>(value => value.Second % 2 == 0)
                .AddMediatorConfiguration(cfg =>
                {
                    foreach (var handlerType in handlerTypes)
                        cfg.Add(handlerType);
                })
                .AddMediator()
                .BuildServiceProvider();

            return (provider.GetRequiredService<IMediator>(), logHandler.Logs);
        }

        internal class ConversionHandler<TRequest, TResponse> : IPipeRequestHandler<Request<TRequest>, TRequest, TResponse, Response<TResponse>>
        {
            private static readonly JsonSerializerOptions options = new JsonSerializerOptions().ConfigureForOperations();

            private readonly ILogger<MediatorTest> logger;

            public ConversionHandler(
                ILogger<MediatorTest> logger
            )
            {
                this.logger = logger;
            }

            public async Task<Response<TResponse>> HandleAsync(
                Request<TRequest> request,
                CancellationToken cancellationToken,
                Func<TRequest, Task<TResponse>> next
            )
            {
                logger.Trace($"Deserialize Request to {typeof(TRequest).Name}");
                var payload = JsonSerializer.Deserialize<TRequest>(request.Value, options);

                var result = await next(payload);

                logger.Trace($"Serialize {typeof(TResponse).Name} to Response");
                return new Response<TResponse>(JsonSerializer.Serialize(result, options));
            }
        }

        internal class Request<T>
        {
            private static readonly JsonSerializerOptions options = new JsonSerializerOptions().ConfigureForOperations();

            public string Value { get; }

            public Request(T value)
            {
                Value = JsonSerializer.Serialize(value, options);
            }
        }

        internal class Response<T>
        {
            private static readonly JsonSerializerOptions options = new JsonSerializerOptions().ConfigureForOperations();

            public T Value { get; }

            public Response(string value)
            {
                Value = JsonSerializer.Deserialize<T>(value, options);
            }
        }

        internal class ValidationHandler<TRequest, TResponse> : IPipeRequestHandler<TRequest, TRequest, TResponse, IBooleanResult<TResponse>>
        {
            private readonly Func<TRequest, bool> validate;
            private readonly ILogger<MediatorTest> logger;

            public ValidationHandler(
                Func<TRequest, bool> validate,
                ILogger<MediatorTest> logger
            )
            {
                this.validate = validate;
                this.logger = logger;
            }

            public async Task<IBooleanResult<TResponse>> HandleAsync(
                TRequest request,
                CancellationToken cancellationToken,
                Func<TRequest, Task<TResponse>> next
            )
            {
                logger.Trace($"Start {typeof(TRequest).Name} validation");
                var result = validate(request) ?
                    Result.Success(default(TResponse) !) :
                    Result.Failure(default(TResponse) !).Error("Validation failed");
                logger.Trace($"Status of {typeof(TRequest).Name} validation: {result.IsSuccess}");
                if (result.HasErrors)
                    return result;

                var response = await next(request);

                return Result.Success(response);
            }
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
                logger.Info(GetType().FullName!);
                logger.Info(request.GetHashCode().ToString());

                var response = new TResponse() { Value = request.Value!.Replace(' ', '_') };

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
                logger.Info(GetType().FullName!);
                logger.Info(request.GetHashCode().ToString());

                return Task.FromResult(new One() { First = request.Value!.Length, Value = request.Value });
            }
        }

        private class Authored<T>
        {
            public int AuthorId { get; set; }
            public T Entity { get; set; } = default !;

            public override int GetHashCode() => 13 * AuthorId.GetHashCode() + Entity!.GetHashCode();
        }

        private class Base
        {
            public string? Value { get; set; }

            public override int GetHashCode() => Value!.GetHashCode();
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