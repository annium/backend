using System.Reflection;
using Annium.Core.Runtime.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IRegistrationBuilder SelectTypes(this IServiceCollection services)
        {
            return new RegistrationBuilder(
                services,
                TypeManager.Instance.Types
            );
        }

        public static IRegistrationBuilder SelectAssemblyTypes(this IServiceCollection services)
        {
            var assembly = Assembly.GetCallingAssembly();

            return new RegistrationBuilder(
               services,
               assembly.GetTypes()
           );
        }
    }
}