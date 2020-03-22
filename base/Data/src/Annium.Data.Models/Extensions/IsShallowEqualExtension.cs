using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Data.Models.Extensions
{
    public static partial class ShallowEqualPlainExtensions
    {
        private static readonly IDictionary<Type, Delegate> Comparers =
            new Dictionary<Type, Delegate>();

        private static readonly IDictionary<Type, LambdaExpression> RawComparers =
            new Dictionary<Type, LambdaExpression>();

        public static bool IsShallowEqual<T>(this T value, T data)
        {
            ResolveComparer(typeof(T));
            var comparer = Comparers[typeof(T)];

            try
            {
                return (bool) comparer.DynamicInvoke(value, data)!;
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        }

        private static LambdaExpression ResolveComparer(Type type)
        {
            lock (RawComparers)
            {
                if (RawComparers.TryGetValue(type, out var comparer))
                    return comparer;

                comparer = BuildComparer(type);
                Comparers[type] = comparer.Compile();

                return RawComparers[type] = comparer;
            }
        }
    }
}