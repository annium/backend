using System;
using System.Linq;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Types;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Handlers;
using Annium.Infrastructure.WebSockets.Server.Internal.Handlers;
using Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Requests;
using Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Subscriptions;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;

namespace Annium.Infrastructure.WebSockets.Server;

public static class MediatorConfigurationExtensions
{
    public static MediatorConfiguration AddWebSocketServerHandlers(
        this MediatorConfiguration cfg,
        ITypeManager tm
    )
    {
        cfg.AddHandler(typeof(PushMessageHandler<>));
        cfg.AddRequestHandlers(tm);
        cfg.AddSubscriptionHandlers(tm);

        return cfg;
    }

    private static void AddRequestHandlers(
        this MediatorConfiguration cfg,
        ITypeManager tm
    )
    {
        cfg.AddHandler(typeof(RequestResponseHandler<,,>));
        // IRequestResponseHandler<TRequest, TResponse, TState>: TRequest -> ResultResponse<TResponse>
        cfg.AddHandlerImplementations(
            tm,
            typeof(IRequestResponseHandler<,,>),
            args => Arr(
                (Context(args[0], args[2]), Generic(typeof(ResultResponse<>), args[1]))
            )
        );

        cfg.AddHandler(typeof(RequestHandler<,>));
        // IRequestHandler<TRequest, TState>: TRequest -> ResultResponse
        cfg.AddHandlerImplementations(
            tm,
            typeof(IRequestHandler<,>),
            args => Arr(
                (Context(args[0], args[1]), typeof(ResultResponse))
            )
        );
    }

    private static void AddSubscriptionHandlers(
        this MediatorConfiguration cfg,
        ITypeManager tm
    )
    {
        cfg.AddHandler(typeof(SubscriptionInitHandler<,,>));
        cfg.AddHandler(typeof(SubscriptionCancelHandler<>));
        // ISubscriptionHandler<TInit, TMessage, TState>:
        // - TInit -> VoidResponse<TCancel, TMessage>
        // - TMessage -> VoidResponse<TInit, TCancel>
        // - TCancel -> VoidResponse<TInit, TMessage>
        cfg.AddHandlerImplementations(
            tm,
            typeof(ISubscriptionHandler<,,>),
            args => Arr(
                (Context(args[0], args[2]), Generic(typeof(VoidResponse<>), args[1])),
                (Context(typeof(SubscriptionCancelRequest), args[2]), typeof(ResultResponse))
            )
        );
    }

    private static void AddHandlerImplementations(
        this MediatorConfiguration cfg,
        ITypeManager tm,
        Type interfaceType,
        Func<Type[], ValueTuple<Type, Type>[]> resolveMatch
    )
    {
        foreach (var handler in tm.GetImplementations(interfaceType))
        {
            cfg.AddHandler(handler);
            var implementations = handler.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType)
                .ToArray();
            foreach (var implementation in implementations)
            {
                var matches = resolveMatch(implementation.GetGenericArguments());
                foreach (var (requestedType, resolvedType) in matches)
                    cfg.AddMatch(requestedType, typeof(AbstractResponseBase), resolvedType);
            }
        }
    }

    // IRequestContext<TRequest, TState> -> RequestContext<TRequest, TState>
    private static Type Context(Type requestType, Type stateType) =>
        typeof(RequestContext<,>).MakeGenericType(requestType, stateType);

    private static Type Generic(Type type, params Type[] args) =>
        type.MakeGenericType(args);

    private static T[] Arr<T>(params T[] items) => items;
}