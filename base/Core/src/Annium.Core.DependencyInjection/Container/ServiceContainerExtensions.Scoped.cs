// using System;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace Annium.Core.DependencyInjection
// {
//     public static partial class ServiceContainerExtensions
//     {
//         #region Add
//
//         public static IServiceContainer AddScoped<TService>(this IServiceContainer container)
//             where TService : class
//         {
//             return container.Add(ServiceDescriptor.Scoped<TService, TService>());
//         }
//
//         public static IServiceContainer AddScoped<TService, TImplementation>(this IServiceContainer container)
//             where TService : class
//             where TImplementation : class, TService
//         {
//             return container.Add(ServiceDescriptor.Scoped<TService, TImplementation>());
//         }
//
//         public static IServiceContainer AddScoped<TService>(
//             this IServiceContainer container,
//             Func<IServiceProvider, TService> implementationFactory
//         )
//             where TService : class
//         {
//             return container.Add(ServiceDescriptor.Scoped(implementationFactory));
//         }
//
//         public static IServiceContainer AddScoped<TService, TImplementation>(
//             this IServiceContainer container,
//             Func<IServiceProvider, TImplementation> implementationFactory
//         )
//             where TService : class
//             where TImplementation : class, TService
//         {
//             return container.Add(ServiceDescriptor.Scoped<TService, TImplementation>(implementationFactory));
//         }
//
//         public static IServiceContainer AddScoped(
//             this IServiceContainer container,
//             Type serviceType
//         )
//         {
//             return container.Add(ServiceDescriptor.Scoped(serviceType, serviceType));
//         }
//
//         public static IServiceContainer AddScoped(
//             this IServiceContainer container,
//             Type serviceType,
//             Func<IServiceProvider, object> implementationFactory
//         )
//         {
//             return container.Add(ServiceDescriptor.Scoped(serviceType, implementationFactory));
//         }
//
//         public static IServiceContainer AddScoped(
//             this IServiceContainer container,
//             Type serviceType,
//             Type implementationType
//         )
//         {
//             return container.Add(ServiceDescriptor.Scoped(serviceType, implementationType));
//         }
//
//         #endregion
//
//         #region TryAdd
//
//         public static IServiceContainer TryAddScoped<TService>(this IServiceContainer container)
//             where TService : class
//         {
//             return container.TryAdd(ServiceDescriptor.Scoped<TService, TService>());
//         }
//
//         public static IServiceContainer TryAddScoped<TService, TImplementation>(this IServiceContainer container)
//             where TService : class
//             where TImplementation : class, TService
//         {
//             return container.TryAdd(ServiceDescriptor.Scoped<TService, TImplementation>());
//         }
//
//         public static IServiceContainer TryAddScoped<TService>(
//             this IServiceContainer container,
//             Func<IServiceProvider, TService> implementationFactory
//         )
//             where TService : class
//         {
//             return container.TryAdd(ServiceDescriptor.Scoped(implementationFactory));
//         }
//
//         public static IServiceContainer TryAddScoped<TService, TImplementation>(
//             this IServiceContainer container,
//             Func<IServiceProvider, TImplementation> implementationFactory
//         )
//             where TService : class
//             where TImplementation : class, TService
//         {
//             return container.TryAdd(ServiceDescriptor.Scoped<TService, TImplementation>(implementationFactory));
//         }
//
//         public static IServiceContainer TryAddScoped(
//             this IServiceContainer container,
//             Type serviceType
//         )
//         {
//             return container.TryAdd(ServiceDescriptor.Scoped(serviceType, serviceType));
//         }
//
//         public static IServiceContainer TryAddScoped(
//             this IServiceContainer container,
//             Type serviceType,
//             Func<IServiceProvider, object> implementationFactory
//         )
//         {
//             return container.TryAdd(ServiceDescriptor.Scoped(serviceType, implementationFactory));
//         }
//
//         public static IServiceContainer TryAddScoped(
//             this IServiceContainer container,
//             Type serviceType,
//             Type implementationType
//         )
//         {
//             return container.TryAdd(ServiceDescriptor.Scoped(serviceType, implementationType));
//         }
//
//         #endregion
//     }
// }