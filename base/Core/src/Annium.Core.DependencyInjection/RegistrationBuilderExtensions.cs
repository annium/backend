using System;
using Annium.Core.Reflection;

namespace Annium.Core.DependencyInjection.New
{
    public static class RegistrationBuilderExtensions
    {
        public static IRegistrationBuilder AssignableTo<T>(this IRegistrationBuilder builder)
            => builder.Where(x => x.IsDerivedFrom(typeof(T)));

        public static IRegistrationBuilder AssignableTo(this IRegistrationBuilder builder, Type baseType)
            => builder.Where(x => x.IsDerivedFrom(baseType));

        public static IRegistrationBuilder StartingWith(this IRegistrationBuilder builder, string prefix)
            => builder.Where(x => x.Name.StartsWith(prefix));

        public static IRegistrationBuilder EndingWith(this IRegistrationBuilder builder, string suffix)
            => builder.Where(x => x.Name.EndsWith(suffix));
    }
}