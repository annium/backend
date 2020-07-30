using System;
using System.Reflection;
using Annium.Core.DependencyInjection.Internal;
using Annium.Core.Runtime.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IRegistrationBuilder AddAllTypes(this IServiceCollection services)
        {
            var typeManager = services.BuildServiceProvider().GetRequiredService<ITypeManager>();

            return new RegistrationBuilder(services, typeManager.Types);
        }

        public static IRegistrationBuilder AddAllTypes(this IServiceCollection services, Assembly assembly)
            => new RegistrationBuilder(services, TypeManager.GetInstance(assembly).Types);

        public static IRegistrationBuilder AddAssemblyTypes(this IServiceCollection services, Assembly assembly)
            => new RegistrationBuilder(services, assembly.GetTypes());

        public static IRegistrationBuilder Add<T>(this IServiceCollection services)
            => new RegistrationBuilder(services, new[] { typeof(T) });

        public static IRegistrationBuilder Add(this IServiceCollection services, params Type[] types)
            => new RegistrationBuilder(services, types);

        public static IServiceCollection Clone(this IServiceCollection services)
        {
            var clone = new ServiceCollection();

            foreach (var descriptor in services)
                clone.Add(descriptor);

            return clone;
        }
    }
}