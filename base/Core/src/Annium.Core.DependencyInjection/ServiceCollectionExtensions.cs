using System.Reflection;
using Annium.Core.DependencyInjection.Internal;
using Annium.Core.Runtime.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IRegistrationBuilder AddAllTypes(this IServiceCollection services) => services.AddAllTypes(Assembly.GetEntryAssembly()!);

        public static IRegistrationBuilder AddAllTypes(this IServiceCollection services, Assembly assembly)
            => new RegistrationBuilder(services, TypeManager.GetInstance(assembly).Types);

        public static IRegistrationBuilder AddAssemblyTypes(this IServiceCollection services)
            => services.AddAssemblyTypes(Assembly.GetCallingAssembly());

        public static IRegistrationBuilder AddAssemblyTypes(this IServiceCollection services, Assembly assembly)
            => new RegistrationBuilder(services, assembly.GetTypes());
    }
}