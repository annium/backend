using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Mesh.Domain;
using Annium.Mesh.Server.Internal.Routing;
using Annium.Reflection;

namespace Annium.Mesh.Server;

/// <summary>
/// Provides configuration options for the mesh server, including API registration and handler management.
/// </summary>
public class ServerConfigurationOptions
{
    /// <summary>
    /// The service container for dependency injection.
    /// </summary>
    private readonly IServiceContainer _container;

    /// <summary>
    /// The route store for managing request and push routes.
    /// </summary>
    private readonly RouteStore _routeStore = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerConfigurationOptions"/> class.
    /// </summary>
    /// <param name="container">The service container for dependency injection.</param>
    internal ServerConfigurationOptions(IServiceContainer container)
    {
        _container = container;
        _container.Add(_routeStore).AsSelf().Singleton();
    }

    /// <summary>
    /// Registers an API with the specified action enum type and version, automatically discovering and registering all handlers.
    /// </summary>
    /// <typeparam name="TAction">The enum type representing actions for this API.</typeparam>
    /// <param name="version">The version of the API to register.</param>
    public void WithApi<TAction>(ushort version)
        where TAction : struct, Enum
    {
        if (typeof(TAction).GetEnumUnderlyingType() != typeof(int))
            throw new InvalidOperationException(
                $"Action {typeof(TAction).FriendlyName()} underlying type must be {nameof(Int32)}"
            );

        RegisterHandlers<TAction>(version);
    }

    /// <summary>
    /// Discovers and registers all handlers for the specified action type and version.
    /// </summary>
    /// <typeparam name="TAction">The enum type representing actions for the API.</typeparam>
    /// <param name="version">The version of the API.</param>
    private void RegisterHandlers<TAction>(ushort version)
        where TAction : struct, Enum
    {
        var implementations = _container
            .GetTypeManager()
            .GetImplementations(typeof(IHandlerBase<TAction>))
            .Where(x =>
                x is { IsClass: true, IsGenericType: false }
                && x.GetProperty(nameof(IHandlerBase<>.Version))!.GetPropertyOrFieldValue<ushort>() == version
            )
            .ToArray();

        var actions = Enum.GetValues<TAction>().ToHashSet();
        var registrations = new Dictionary<TAction, Type>();

        foreach (var implementation in implementations)
        {
            var action = implementation.GetProperty(nameof(IHandlerBase<>.Action))!.GetPropertyOrFieldValue<TAction>();
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

    /// <summary>
    /// Registers a specific handler implementation by determining its type and configuring appropriate routes.
    /// </summary>
    /// <param name="actionKey">The action key identifying the handler.</param>
    /// <param name="implementation">The handler implementation type.</param>
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

    /// <summary>
    /// Attempts to register a request handler implementation.
    /// </summary>
    /// <param name="actionKey">The action key identifying the handler.</param>
    /// <param name="implementation">The handler implementation type.</param>
    /// <returns>True if the handler was successfully registered as a request handler; otherwise, false.</returns>
    private bool RegisterRequestHandler(ActionKey actionKey, Type implementation)
    {
        if (
            !TryResolveHandler(
                implementation,
                typeof(IRequestHandler<,>),
                nameof(IRequestHandler<,>.HandleAsync),
                out var info
            )
        )
            return false;

        var resultProperty = info.Handle.ReturnType.GetProperty(nameof(Task<>.Result))!;
        _routeStore.RequestRoutes.Register(
            actionKey,
            new RequestRoute(implementation, info.Handle, info.Args[1], resultProperty)
        );

        return true;
    }

    /// <summary>
    /// Attempts to register a request-response handler implementation.
    /// </summary>
    /// <param name="actionKey">The action key identifying the handler.</param>
    /// <param name="implementation">The handler implementation type.</param>
    /// <returns>True if the handler was successfully registered as a request-response handler; otherwise, false.</returns>
    private bool RegisterRequestResponseHandler(ActionKey actionKey, Type implementation)
    {
        if (
            !TryResolveHandler(
                implementation,
                typeof(IRequestResponseHandler<,,>),
                nameof(IRequestResponseHandler<,,>.HandleAsync),
                out var info
            )
        )
            return false;

        var resultProperty = info.Handle.ReturnType.GetProperty(nameof(Task<>.Result))!;
        _routeStore.RequestRoutes.Register(
            actionKey,
            new RequestRoute(implementation, info.Handle, info.Args[1], resultProperty)
        );

        return true;
    }

    /// <summary>
    /// Attempts to register a push handler implementation.
    /// </summary>
    /// <param name="actionKey">The action key identifying the handler.</param>
    /// <param name="implementation">The handler implementation type.</param>
    /// <returns>True if the handler was successfully registered as a push handler; otherwise, false.</returns>
    private bool RegisterPushHandler(ActionKey actionKey, Type implementation)
    {
        if (!TryResolveHandler(implementation, typeof(IPushHandler<,>), nameof(IPushHandler<,>.RunAsync), out var info))
            return false;

        _routeStore.PushRoutes.Register(actionKey, new PushRoute(implementation, info.Handle, info.Args[1]));

        return true;
    }

    /// <summary>
    /// Attempts to resolve handler information from a handler type.
    /// </summary>
    /// <param name="handlerType">The handler type to analyze.</param>
    /// <param name="targetType">The target interface type to look for.</param>
    /// <param name="handleName">The name of the handle method.</param>
    /// <param name="info">When this method returns, contains the handler information if found.</param>
    /// <returns>True if the handler information was successfully resolved; otherwise, false.</returns>
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

    /// <summary>
    /// Contains information about a handler method and its generic type arguments.
    /// </summary>
    /// <param name="Handle">The method info for the handler method.</param>
    /// <param name="Args">The generic type arguments for the handler.</param>
    private record HandlerInfo(MethodInfo Handle, Type[] Args);
}
