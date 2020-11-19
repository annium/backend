using System;

namespace Annium.Core.DependencyInjection.Obsolete
{
    public static class RegistrationBuilderExtensions
    {
        [Obsolete]
        public static IRegistrationBuilder AssignableTo<T>(this IRegistrationBuilder builder)
            => builder.Where(typeof(T).IsAssignableFrom);

        [Obsolete]
        public static IRegistrationBuilder AssignableTo(this IRegistrationBuilder builder, Type baseType)
            => builder.Where(baseType.IsAssignableFrom);

        [Obsolete]
        public static IRegistrationBuilder StartingWith(this IRegistrationBuilder builder, string prefix)
            => builder.Where(x => x.Name.StartsWith(prefix));

        [Obsolete]
        public static IRegistrationBuilder EndingWith(this IRegistrationBuilder builder, string suffix)
            => builder.Where(x => x.Name.EndsWith(suffix));
    }
}