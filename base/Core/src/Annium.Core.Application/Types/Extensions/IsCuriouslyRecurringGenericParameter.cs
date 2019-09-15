using System;
using System.Linq;

namespace Annium.Core.Application
{
    public static class IsCuriouslyRecurringGenericParameterExtension
    {
        public static bool IsCuriouslyRecurringGenericParameter(this Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (!type.IsGenericParameter)
                return false;

            return type.GetGenericParameterConstraints()
                .Any(constraint => constraint.GetGenericArguments().Contains(type));
        }
    }
}