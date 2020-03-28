using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Annium.Data.Models.Extensions
{
    public static partial class IsShallowEqualExtensions
    {
        private static LambdaExpression BuildComparer(Type type)
        {
            // if Equality operator is overriden - return it's call
            var equalityOperatorMethod = ResolveEqualityOperatorMethod(type);
            if (equalityOperatorMethod != null)
                return BuildEqualityOperatorComparer(type, equalityOperatorMethod);

            // if Equals is overriden - return it's call
            var equalsMethod = ResolveEqualsMethod(type);
            if (equalsMethod != null)
                return BuildEqualsComparer(type, equalsMethod);

            // generic collections
            if (type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                return BuildGenericEnumerableComparer(type);

            // non-generic collections
            if (type.GetInterfaces().Any(x => x == typeof(IEnumerable)))
                return BuildNonGenericEnumerableComparer(type);

            return BuildPropertyFieldComparer(type);
        }
    }
}