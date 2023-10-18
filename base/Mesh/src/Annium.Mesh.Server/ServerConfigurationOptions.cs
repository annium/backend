using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Annium.Core.DependencyInjection;
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
            throw new InvalidOperationException($"Action {typeof(TAction).FriendlyName()} underlying type must be {nameof(Int32)}");

        RegisterHandlers<TAction>(version);
    }

    private void RegisterHandlers<TAction>(ushort version)
        where TAction : struct, Enum
    {
        var implementations = _container.GetTypeManager().GetImplementations(typeof(IHandlerBase<TAction>))
            .Where(x =>
                x is { IsClass: true, IsGenericType: false } &&
                x.GetProperty(nameof(IHandlerBase<TAction>.Version))!.GetPropertyOrFieldValue<ushort>() == version
            )
            .ToArray();

        var actions = Enum.GetValues<TAction>().ToHashSet();

        foreach (var implementation in implementations)
        {
            var action = implementation.GetProperty(nameof(IHandlerBase<TAction>.Action))!.GetPropertyOrFieldValue<TAction>();
            if (!actions.Remove(action))
                throw new InvalidOperationException($"Action {action} is outside of known actions");

            var actionKey = new ActionKey(version, Convert.ToInt32(action), action.ToString());
            RegisterHandler(actionKey, implementation);
        }

        if (actions.Count > 0)
            throw new InvalidOperationException($"Actions {string.Join(", ", actions)} are not mapped to any handlers");
    }

    private void RegisterHandler(ActionKey actionKey, Type implementation)
    {
        if (TryResolveHandler(implementation, typeof(IRequestHandler<,>), out var args))
        {
            _container.Add(implementation).AsSelf().Singleton();
            _routeStore.RequestRoutes.Register(actionKey, new RequestData(implementation, args[1]));
            return;
        }

        if (TryResolveHandler(implementation, typeof(IRequestResponseHandler<,,>), out args))
        {
            _routeStore.RequestResponseRoutes.Register(actionKey, new RequestResponseData(implementation, args[1], args[2]));
            return;
        }

        throw new InvalidOperationException($"Failed to resolve handler type {implementation.FriendlyName()} ({actionKey})");
    }

    private static bool TryResolveHandler(Type handlerType, Type targetType, [NotNullWhen(true)] out Type[]? args)
    {
        var implementationType = handlerType.GetInterfaces()
            .SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == targetType);

        args = null;
        if (implementationType is null)
            return false;

        args = implementationType.GetGenericArguments();
        return true;
    }
}