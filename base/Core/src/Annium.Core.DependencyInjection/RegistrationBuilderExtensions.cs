using System;

namespace Annium.Core.DependencyInjection
{
    public static class RegistrationBuilderExtensions
    {
        public static IRegistrationBuilder AssignableTo<T>(this IRegistrationBuilder builder)
            => builder.Where(typeof(T).IsAssignableFrom);

        public static IRegistrationBuilder AssignableTo(this IRegistrationBuilder builder, Type baseType)
            => builder.Where(baseType.IsAssignableFrom);

        public static IRegistrationBuilder StartingWith(this IRegistrationBuilder builder, string prefix)
            => builder.Where(x => x.Name.StartsWith(prefix));

        public static IRegistrationBuilder EndingWith(this IRegistrationBuilder builder, string suffix)
            => builder.Where(x => x.Name.EndsWith(suffix));
    }
}