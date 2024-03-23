using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Domain;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Internal.Routing;
using Annium.Reflection;

namespace Annium.Mesh.Server;

public class ServerConfigurationOptions
{
    private readonly IServiceContainer _container;
    private readonly RouteStore _routeStore = new();

    internal ServerConfigurationOptions(IServiceContainer container)
    {
        _container = container;
        _container.Add(_routeStore).AsSelf().Singleton();
    }

    public void WithApi<TAction>(ushort version)
        where TAction : struct, Enum
    {
        if (typeof(TAction).GetEnumUnderlyingType() != typeof(int))
            throw new InvalidOperationException(
                $"Action {typeof(TAction).FriendlyName()} underlying type must be {nameof(Int32)}"
            );

        RegisterHandlers<TAction>(version);
    }

    private void RegisterHandlers<TAction>(ushort version)
        where TAction : struct, Enum
    {
        var implementations = _container
            .GetTypeManager()
            .GetImplementations(typeof(IHandlerBase<TAction>))
            .Where(x =>
                x is { IsClass: true, IsGenericType: false }
                && x.GetProperty(nameof(IHandlerBase<TAction>.Version))!.GetPropertyOrFieldValue<ushort>() == version
            )
            .ToArray();

        var actions = Enum.GetValues<TAction>().ToHashSet();
        var registrations = new Dictionary<TAction, Type>();

        foreach (var implementation in implementations)
        {
            var action = implementation
                .GetProperty(nameof(IHandlerBase<TAction>.Action))!
                .GetPropertyOrFieldValue<TAction>();
            if (!actions.Contains(action))
                throw new InvalidOperationException($"Action {action} is outside of known action values");

            if (registrations.TryGetValue(action, out var existingImplementation))
                throw new InvalidOperationException(
                    $"Action {action} is already used by {existingImplementation.FriendlyName()}"
                );

            var actionKey = new ActionKey(version, Convert.ToInt32(action));
            RegisterHandler(actionKey, implementation);
            _container.Add(implementation).AsSelf().Scoped();
            registrations.Add(action, implementation);
        }
    }

    private void RegisterHandler(ActionKey actionKey, Type implementation)
    {
        if (RegisterRequestHandler(actionKey, implementation))
            return;

        if (RegisterRequestResponseHandler(actionKey, implementation))
            return;

        if (RegisterPushHandler(actionKey, implementation))
            return;

        throw new InvalidOperationException(
            $"Failed to resolve handler type {implementation.FriendlyName()} ({actionKey})"
        );
    }

    private bool RegisterRequestHandler(ActionKey actionKey, Type implementation)
    {
        if (
            !TryResolveHandler(
                implementation,
                typeof(IRequestHandler<,>),
                nameof(IRequestHandler<MessageType, object>.HandleAsync),
                out var info
            )
        )
            return false;

        var resultProperty = info.Handle.ReturnType.GetProperty(nameof(Task<object>.Result))!;
        _routeStore.RequestRoutes.Register(
            actionKey,
            new RequestRoute(implementation, info.Handle, info.Args[1], resultProperty)
        );

        return true;
    }

    private bool RegisterRequestResponseHandler(ActionKey actionKey, Type implementation)
    {
        if (
            !TryResolveHandler(
                implementation,
                typeof(IRequestResponseHandler<,,>),
                nameof(IRequestResponseHandler<MessageType, object, object>.HandleAsync),
                out var info
            )
        )
            return false;

        var resultProperty = info.Handle.ReturnType.GetProperty(nameof(Task<object>.Result))!;
        _routeStore.RequestRoutes.Register(
            actionKey,
            new RequestRoute(implementation, info.Handle, info.Args[1], resultProperty)
        );

        return true;
    }

    private bool RegisterPushHandler(ActionKey actionKey, Type implementation)
    {
        if (
            !TryResolveHandler(
                implementation,
                typeof(IPushHandler<,>),
                nameof(IPushHandler<MessageType, object>.RunAsync),
                out var info
            )
        )
            return false;

        _routeStore.PushRoutes.Register(actionKey, new PushRoute(implementation, info.Handle, info.Args[1]));

        return true;
    }

    private static bool TryResolveHandler(
        Type handlerType,
        Type targetType,
        string handleName,
        [NotNullWhen(true)] out HandlerInfo? info
    )
    {
        var implementationType = handlerType
            .GetInterfaces()
            .SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == targetType);

        info = null;
        if (implementationType is null)
            return false;

        var handle = implementationType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(x => x.Name == handleName);
        var args = implementationType.GetGenericArguments();
        info = new HandlerInfo(handle, args);
        return true;
    }

    private record HandlerInfo(MethodInfo Handle, Type[] Args);
}
