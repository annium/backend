using System;
using Annium.Core.Mediator;
using Annium.Core.Reflection;
using Annium.Core.Runtime.Types;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Handlers;
using Annium.Infrastructure.WebSockets.Server.Internal.Handlers;
using Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Requests;
using Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Subscriptions;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;

namespace Annium.Infrastructure.WebSockets.Server
{
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
            cfg.AddHandler(typeof(RequestResponseHandler<,>));
            // IRequestHandler<TRequest, TResponse>: TRequest -> ResultResponse<TResponse>
            cfg.AddHandlerImplementations(
                tm,
                typeof(IRequestResponseHandler<,>),
                args => Arr(
                    (Context(args[0]), Generic(typeof(ResultResponse<>), args[1]))
                )
            );

            cfg.AddHandler(typeof(RequestHandler<>));
            // IRequestHandler<TRequest>: TRequest -> ResultResponse
            cfg.AddHandlerImplementations(
                tm,
                typeof(IRequestHandler<>),
                args => Arr(
                    (Context(args[0]), typeof(ResultResponse))
                )
            );
        }

        private static void AddSubscriptionHandlers(
            this MediatorConfiguration cfg,
            ITypeManager tm
        )
        {
            cfg.AddHandler(typeof(SubscriptionInitHandler<,>));
            cfg.AddHandler(typeof(SubscriptionCancelHandler<,>));
            // ISubscriptionHandler<TInit, TMessage, TCancel>:
            // - TInit -> VoidResponse<TCancel, TMessage>
            // - TMessage -> VoidResponse<TInit, TCancel>
            // - TCancel -> VoidResponse<TInit, TMessage>
            cfg.AddHandlerImplementations(
                tm,
                typeof(ISubscriptionHandler<,>),
                args => Arr(
                    (Context(args[0]), Generic(typeof(VoidResponse<>), args[1])),
                    (Context(typeof(SubscriptionCancelRequest)), Generic(typeof(MetaResponse<,,>), args[0], args[1], typeof(ResultResponse)))
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
                var matches = resolveMatch(handler.GetTargetImplementation(interfaceType)!.GetGenericArguments());
                foreach (var (requestedType, resolvedType) in matches)
                    cfg.AddMatch(requestedType, typeof(AbstractResponseBase), resolvedType);
            }
        }

        // IRequestContext<T> -> RequestContext<T>
        private static Type Context(Type type) =>
            typeof(RequestContext<>).MakeGenericType(type);

        private static Type Generic(Type type, params Type[] args) =>
            type.MakeGenericType(args);

        private static T[] Arr<T>(params T[] items) => items;
    }
}