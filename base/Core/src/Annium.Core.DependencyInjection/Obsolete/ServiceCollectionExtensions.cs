using System;
using System.Reflection;
using Annium.Core.DependencyInjection.Obsolete.Internal;
using Annium.Core.Runtime.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Annium.Core.DependencyInjection.Obsolete
{
    public static class ServiceCollectionExtensions
    {
        [Obsolete]
        public static IRegistrationBuilder AddAllTypes(this IServiceCollection services)
        {
            var typeManager = services.GetTypeManager();

            return new RegistrationBuilder(services, typeManager.Types);
        }

        [Obsolete]
        public static IRegistrationBuilder AddAllTypes(this IServiceCollection services, Assembly assembly, bool tryLoadReferences)
            => new RegistrationBuilder(services, TypeManager.GetInstance(assembly, tryLoadReferences).Types);

        [Obsolete]
        public static IRegistrationBuilder AddAssemblyTypes(this IServiceCollection services, Assembly assembly)
            => new RegistrationBuilder(services, assembly.GetTypes());

        [Obsolete]
        public static IRegistrationBuilder Add<T>(this IServiceCollection services)
            => new RegistrationBuilder(services, new[] { typeof(T) });

        [Obsolete]
        public static IRegistrationBuilder Add(this IServiceCollection services, params Type[] types)
            => new RegistrationBuilder(services, types);

        [Obsolete]
        public static IServiceCollection Clone(this IServiceCollection services)
        {
            var clone = new ServiceCollection();

            foreach (var descriptor in services)
                clone.Add(descriptor);

            return clone;
        }
    }
}