using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Core.Mapper;

namespace Annium.Data.Models.Extensions
{
    public static partial class IsShallowEqualExtensions
    {
        private static readonly object Locker = new object();

        private static readonly HashSet<Type> ComparersInProgress =
            new HashSet<Type>();

        private static readonly IDictionary<Type, Delegate> Comparers =
            new Dictionary<Type, Delegate>();

        private static readonly IDictionary<Type, LambdaExpression> RawComparers =
            new Dictionary<Type, LambdaExpression>();

        public static bool IsShallowEqual<T, D>(this T value, D data)
        {
            var type = typeof(D);

            if (type.IsClass)
            {
                // if data is null, simply compare to null
                if (data is null)
                    return value is null;

                // if data is not null, but value - is, return false
                if (value is null)
                    return false;

                // for reference equality - return true
                if (ReferenceEquals(value, data))
                    return true;
            }

            // if compared as objects - need to resolve target real type
            if (type == typeof(object))
                type = data.GetType();

            // as far as base object class has no properties, consider objects to be shallowly equal
            if (type == typeof(object))
                return true;

            var comparable = Mapper.Map(value!, type);

            lock (Locker)
            {
                try
                {
                    ResolveComparer(type);
                }
                finally
                {
                    ComparersInProgress.Clear();
                }
            }

            var comparer = Comparers[type];
            // var str = RawComparers[type].ToReadableString();

            try
            {
                return (bool) comparer.DynamicInvoke(comparable, data)!;
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        }

        private static LambdaExpression ResolveComparer(Type type)
        {
            if (RawComparers.TryGetValue(type, out var comparer))
                return comparer;

            if (!ComparersInProgress.Add(type))
                return BuildExtensionCallComparer(type);

            comparer = RawComparers[type] = BuildComparer(type);
            ComparersInProgress.Remove(type);

            Comparers[type] = comparer.Compile();

            return comparer;
        }
    }
}