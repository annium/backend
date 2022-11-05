using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Data.Operations;
using Annium.Logging.Abstractions;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mediator.Tests;

public class MediatorTest
{
    [Fact]
    public async Task SingleClosedHandler_Works()
    {
        // arrange
        var (mediator, logs) = GetMediator(cfg => cfg.AddHandler(typeof(ClosedFinalHandler)));
        var request = new Base { Value = "base" };

        // act
        var response = await mediator.SendAsync<One>(request);

        // assert
        response.GetHashCode().Is(new One { First = request.Value.Length, Value = request.Value }.GetHashCode());
        logs.Has(2);
        logs.At(0).Message.Is(typeof(ClosedFinalHandler).FullName);
        logs.At(1).Message.Is(request.GetHashCode().ToString());
    }

    [Fact]
    public async Task SingleOpenHandler_WithExpectedParameters_Works()
    {
        // arrange
        var (mediator, logs) = GetMediator(cfg => cfg.AddHandler(typeof(OpenFinalHandler<,>)));
        var request = new Two { Second = 2, Value = "one two three" };

        // act
        var response = await mediator.SendAsync<Base>(request);

        // assert
        response.GetHashCode().Is(new Base { Value = "one_two_three" }.GetHashCode());
        logs.Has(2);
        logs.At(0).Message.Is(typeof(OpenFinalHandler<Two, Base>).FriendlyName());
        logs.At(1).Message.Is(request.GetHashCode().ToString());
    }

    [Fact]
    public async Task ChainOfHandlers_WithExpectedParameters_Works()
    {
        // arrange
        var (mediator, logs) = GetMediator(cfg => cfg
            .AddHandler(typeof(ConversionHandler<,>))
            .AddHandler(typeof(ValidationHandler<,>))
            .AddHandler(typeof(OpenFinalHandler<,>))
        );
        var request = new Two { Second = 2, Value = "one two three" };
        var payload = new Request<Two>(request);

        // act
        var response = (await mediator.SendAsync<Response<IBooleanResult<Base>>>(payload)).Value;

        // assert
        response.IsSuccess.IsTrue();
        response.Data.GetHashCode().Is(new Base { Value = "one_two_three" }.GetHashCode());
        logs.Has(6);
        logs.At(0).Message.Is($"Deserialize Request to {typeof(Two).FriendlyName()}");
        logs.At(1).Message.Is($"Start {typeof(Two).FriendlyName()} validation");
        logs.At(2).Message.Is($"Status of {typeof(Two).FriendlyName()} validation: {true}");
        logs.At(3).Message.Is(typeof(OpenFinalHandler<Two, Base>).FriendlyName());
        logs.At(4).Message.Is(request.GetHashCode().ToString());
        logs.At(5).Message.Is($"Serialize {typeof(IBooleanResult<Base>).FriendlyName()} to Response");
    }

    [Fact]
    public async Task ChainOfHandlers_WithRegisteredResponse_Works()
    {
        // arrange
        var (mediator, logs) = GetMediator(cfg => cfg
            .AddHandler(typeof(ConversionHandler<,>))
            .AddHandler(typeof(ValidationHandler<,>))
            .AddHandler(typeof(OpenFinalHandler<,>))
            .AddMatch(typeof(Request<Two>), typeof(IResponse), typeof(Response<IBooleanResult<Base>>))
        );
        var request = new Two { Second = 2, Value = "one two three" };
        var payload = new Request<Two>(request);

        // act
        var response = (await mediator.SendAsync<IResponse>(payload)).As<Response<IBooleanResult<Base>>>().Value;

        // assert
        response.IsSuccess.IsTrue();
        response.Data.GetHashCode().Is(new Base { Value = "one_two_three" }.GetHashCode());
        logs.Has(6);
        logs.At(0).Message.Is($"Deserialize Request to {typeof(Two).FriendlyName()}");
        logs.At(1).Message.Is($"Start {typeof(Two).FriendlyName()} validation");
        logs.At(2).Message.Is($"Status of {typeof(Two).FriendlyName()} validation: {true}");
        logs.At(3).Message.Is(typeof(OpenFinalHandler<Two, Base>).FriendlyName());
        logs.At(4).Message.Is(request.GetHashCode().ToString());
        logs.At(5).Message.Is($"Serialize {typeof(IBooleanResult<Base>).FriendlyName()} to Response");
    }

    private ValueTuple<IMediator, IReadOnlyList<LogMessage<DefaultLogContext>>> GetMediator(Action<MediatorConfiguration> configure)
    {
        var logHandler = new InMemoryLogHandler<DefaultLogContext>();

        var container = new ServiceContainer();
        container.AddTime().WithRealTime().SetDefault();
        container.Add<Func<One, bool>>(value => value.First % 2 == 1).AsSelf().Singleton();
        container.Add<Func<Two, bool>>(value => value.Second % 2 == 0).AsSelf().Singleton();
        container.AddLogging();
        container.AddMediatorConfiguration(configure);
        container.AddMediator();

        var provider = container.BuildServiceProvider();

        provider.UseLogging(route => route
            .For(m =>
                m.Source.StartsWith("ConversionHandler") ||
                m.Source.StartsWith("ValidationHandler") ||
                m.Source.StartsWith("OpenFinalHandler") ||
                m.Source.StartsWith("ClosedFinalHandler")
            )
            .UseInMemory(logHandler)
        );

        return (provider.Resolve<IMediator>(), logHandler.Logs);
    }

    internal class ConversionHandler<TRequest, TResponse> :
        IPipeRequestHandler<Request<TRequest>, TRequest, TResponse, Response<TResponse>>,
        ILogSubject<ConversionHandler<TRequest, TResponse>>
    {
        private static readonly JsonSerializerOptions
            Options = new JsonSerializerOptions().ConfigureForOperations();

        public ILogger<ConversionHandler<TRequest, TResponse>> Logger { get; }

        public ConversionHandler(
            ILogger<ConversionHandler<TRequest, TResponse>> logger
        )
        {
            Logger = logger;
        }

        public async Task<Response<TResponse>> HandleAsync(
            Request<TRequest> request,
            CancellationToken ct,
            Func<TRequest, CancellationToken, Task<TResponse>> next
        )
        {
            this.Log().Trace($"Deserialize Request to {typeof(TRequest).FriendlyName()}");
            var payload = JsonSerializer.Deserialize<TRequest>(request.Value, Options)!;

            var result = await next(payload, ct);

            this.Log().Trace($"Serialize {typeof(TResponse).FriendlyName()} to Response");
            return new Response<TResponse>(JsonSerializer.Serialize(result, Options));
        }
    }

    internal class Request<T>
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions().ConfigureForOperations();

        public string Value { get; }

        public Request(T value)
        {
            Value = JsonSerializer.Serialize(value, Options);
        }
    }

    internal class Response<T> : IResponse
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions().ConfigureForOperations();

        public T Value { get; }

        public Response(string value)
        {
            Value = JsonSerializer.Deserialize<T>(value, Options)!;
        }
    }

    internal interface IResponse
    {
    }

    internal class ValidationHandler<TRequest, TResponse> :
        IPipeRequestHandler<TRequest, TRequest, TResponse, IBooleanResult<TResponse>>,
        ILogSubject<ValidationHandler<TRequest, TResponse>>
    {
        public ILogger<ValidationHandler<TRequest, TResponse>> Logger { get; }
        private readonly Func<TRequest, bool> _validate;

        public ValidationHandler(
            Func<TRequest, bool> validate,
            ILogger<ValidationHandler<TRequest, TResponse>> logger
        )
        {
            _validate = validate;
            Logger = logger;
        }

        public async Task<IBooleanResult<TResponse>> HandleAsync(
            TRequest request,
            CancellationToken ct,
            Func<TRequest, CancellationToken, Task<TResponse>> next
        )
        {
            this.Log().Trace($"Start {typeof(TRequest).FriendlyName()} validation");
            var result = _validate(request)
                ? Result.Success(default(TResponse)!)
                : Result.Failure(default(TResponse)!).Error("Validation failed");
            this.Log().Trace($"Status of {typeof(TRequest).FriendlyName()} validation: {result.IsSuccess}");
            if (result.HasErrors)
                return result;

            var response = await next(request, ct);

            return Result.Success(response);
        }
    }

    private class OpenFinalHandler<TRequest, TResponse> : IFinalRequestHandler<TRequest, TResponse>, ILogSubject<OpenFinalHandler<TRequest, TResponse>>
        where TRequest : TResponse
        where TResponse : Base, new()
    {
        public ILogger<OpenFinalHandler<TRequest, TResponse>> Logger { get; }

        public OpenFinalHandler(
            ILogger<OpenFinalHandler<TRequest, TResponse>> logger
        )
        {
            Logger = logger;
        }

        public Task<TResponse> HandleAsync(
            TRequest request,
            CancellationToken ct
        )
        {
            this.Log().Info(GetType().FriendlyName());
            this.Log().Trace(request.GetHashCode().ToString());

            var response = new TResponse { Value = request.Value!.Replace(' ', '_') };

            return Task.FromResult(response);
        }
    }

    private class ClosedFinalHandler : IFinalRequestHandler<Base, One>, ILogSubject<ClosedFinalHandler>
    {
        public ILogger<ClosedFinalHandler> Logger { get; }

        public ClosedFinalHandler(
            ILogger<ClosedFinalHandler> logger
        )
        {
            Logger = logger;
        }

        public Task<One> HandleAsync(
            Base request,
            CancellationToken ct
        )
        {
            this.Log().Trace(GetType().FullName!);
            this.Log().Trace(request.GetHashCode().ToString());

            return Task.FromResult(new One { First = request.Value!.Length, Value = request.Value });
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