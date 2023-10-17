using System;
using System.Linq;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Types;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Internal.Handlers;
using Annium.Mesh.Server.Internal.Handlers.Requests;
using Annium.Mesh.Server.Internal.Handlers.Subscriptions;
using Annium.Mesh.Server.Internal.Models;
using Annium.Mesh.Server.Internal.Responses;

namespace Annium.Mesh.Server;

public static class MediatorConfigurationExtensions
{
    public static MediatorConfiguration AddMeshServerHandlers(
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
        cfg.AddHandler(typeof(RequestResponseHandler<,>));
        // IRequestResponseHandler<TRequest, TResponse>: TRequest -> ResultResponse<TResponse>
        cfg.AddHandlerImplementations(
            tm,
            typeof(IRequestResponseHandler<,>),
            args => Arr(
                (Context(args[0]), Generic(typeof(ResultResponseObsolete<>), args[1]))
            )
        );

        cfg.AddHandler(typeof(RequestHandler<>));
        // IRequestHandler<TRequest>: TRequest -> ResultResponse
        cfg.AddHandlerImplementations(
            tm,
            typeof(IRequestHandler<>),
            args => Arr(
                (Context(args[0]), typeof(ResultResponseObsolete))
            )
        );
    }

    private static void AddSubscriptionHandlers(
        this MediatorConfiguration cfg,
        ITypeManager tm
    )
    {
        cfg.AddHandler(typeof(SubscriptionInitHandler<,>));
        cfg.AddHandler(typeof(SubscriptionCancelHandler));
        // ISubscriptionHandler<TInit, TMessage>:
        // - TInit -> VoidResponse<TCancel, TMessage>
        // - TMessage -> VoidResponse<TInit, TCancel>
        // - TCancel -> VoidResponse<TInit, TMessage>
        cfg.AddHandlerImplementations(
            tm,
            typeof(ISubscriptionHandler<,>),
            args => Arr(
                (Context(args[0]), Generic(typeof(VoidResponse<>), args[1])),
                (Context(typeof(SubscriptionCancelRequestObsolete)), typeof(ResultResponseObsolete))
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
                    cfg.AddMatch(requestedType, typeof(AbstractResponseBaseObsolete), resolvedType);
            }
        }
    }

    // IRequestContext<TRequest> -> RequestContext<TRequest>
    private static Type Context(Type requestType) =>
        typeof(RequestContext<>).MakeGenericType(requestType);

    private static Type Generic(Type type, params Type[] args) =>
        type.MakeGenericType(args);

    private static T[] Arr<T>(params T[] items) => items;
}