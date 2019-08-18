using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Core.Mediator
{

    public interface IPipeRequestHandler<TRequestIn, TRequestOut, TResponseIn, TResponseOut>:
        IRequestHandlerInput<TRequestIn, TResponseOut>,
        IRequestHandlerOutput<TRequestOut, TResponseIn>
        {
            Task<TResponseOut> HandleAsync(
                TRequestIn request,
                CancellationToken cancellationToken,
                Func<TRequestOut, Task<TResponseIn>> next
            );
        }

    public interface IFinalRequestHandler<TRequest, TResponse>:
        IRequestHandlerInput<TRequest, TResponse>
        {
            Task<TResponse> HandleAsync(
                TRequest request,
                CancellationToken cancellationToken
            );
        }

    public interface IRequestHandlerInput<TRequestIn, TResponseOut> { }

    public interface IRequestHandlerOutput<TRequestOut, TResponseIn> { }

    // public interface IRequestHandler<TRequest, TResponse>
    // {
    //     Task<TResponse> HandleAsync(
    //         TRequest request,
    //         CancellationToken cancellationToken,
    //         Func<Task<TResponse>> next
    //     );
    // }

    // public interface IRequestHandler<TRequest>
    // {
    //     Task HandleAsync(
    //         TRequest request,
    //         CancellationToken cancellationToken,
    //         Func<Task> next
    //     );
    // }

    // public interface IRequestProcessor<TInput, TOutput>
    // {
    //     Task<TOutput> ProcessRequestAsync(TInput input);
    // }

    // public interface IResponseProcessor<TInput, TOutput>
    // {
    //     Task<TOutput> ProcessResponseAsync(TInput input);
    // }

    // public interface IRequestPrePostHandler<TRequestIn, TRequestOut, TResponseIn, TResponseOut>:
    //     // where TRequestIn : IRequest<TResponseOut>
    //     // where TRequestOut : IRequest<TResponseIn>
    //     {
    //         Task<TRequestOut> PreProcessAsync(TRequestIn request);

    //         Task<TResponseOut> HandleAsync(
    //             TRequestOut request,
    //             CancellationToken cancellationToken,
    //             Func<Task<TResponseIn>> next
    //         );

    //         Task<TResponseOut> PostProcessAsync(TRequestIn request);
    //     }

    // public interface IRequestPreHandler<TRequestIn, TRequestOut, TResponse>
    //     // where TRequestIn : IRequest<TResponse>
    //     // where TRequestOut : IRequest<TResponse>
    //     {
    //         Task<TRequestOut> PreProcessAsync(TRequestIn request);

    //         Task<TResponse> HandleAsync(
    //             TRequestOut request,
    //             CancellationToken cancellationToken,
    //             Func<Task<TResponse>> next
    //         );
    //     }

    // public interface IRequestPostHandler<TRequest, TResponseIn, TResponseOut>
    //     // where TRequest : IRequest<TResponseIn>
    //     {
    //         Task<TResponseOut> HandleAsync(
    //             TRequest request,
    //             CancellationToken cancellationToken,
    //             Func<Task<TResponseIn>> next
    //         );

    //         Task<TResponseOut> PostProcessAsync(TRequest request);
    //     }

    // public interface IRequestHandler<TRequest, TResponse>
    //     // where TRequest : IRequest<TResponse>
    //     {
    //         Task<TResponse> HandleAsync(
    //             TRequest request,
    //             CancellationToken cancellationToken,
    //             Func<Task<TResponse>> next
    //         );
    //     }
}