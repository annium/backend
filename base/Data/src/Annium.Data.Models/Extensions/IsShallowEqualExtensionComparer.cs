using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Core.Reflection;
using Annium.Extensions.Primitives;

namespace Annium.Data.Models.Extensions
{
    public static partial class ShallowEqualPlainExtensions
    {
        private static LambdaExpression BuildComparer(Type type)
        {
            // if IEquatable is implemented - return it's call
            var equals = type.GetMethods().FirstOrDefault(x => IsEqualsMethod(x, type));
            if (equals != null)
            {
                var a = Expression.Parameter(type);
                var b = Expression.Parameter(type);

                return Expression.Lambda(Expression.Call(a, equals, b), a, b);
            }

            // if Equals is overriden - return it's call
            equals = type.GetMethods().FirstOrDefault(x => IsEqualsMethod(x, typeof(object)) && x.DeclaringType == type);
            if (equals != null)
            {
                var a = Expression.Parameter(type);
                var b = Expression.Parameter(type);

                return Expression.Lambda(Expression.Call(a, equals, Expression.Convert(b, typeof(object))), a, b);
            }

            // Array
            if (type.IsArray)
                return BuildSequentialCollectionComparer(
                    type,
                    type.GetElementType()!,
                    x => Expression.ArrayLength(x),
                    (x, i) => Expression.ArrayIndex(x, i)
                );

            // IList
            var implementation = type.GetTargetImplementation(typeof(IList<>))!;
            if (implementation != null)
            {
                var elementType = implementation.GenericTypeArguments[0];
                var getCount = GetCountPropertyGetter(type.GetTargetImplementation(typeof(ICollection<>)!));
                var getElement = GetIndexProperty(type, typeof(int));

                return BuildSequentialCollectionComparer(
                    type,
                    elementType,
                    x => Expression.Call(x, getCount),
                    (x, i) => Expression.Property(x, getElement, i)
                );
            }

            // IDictionary
            implementation = type.GetTargetImplementation(typeof(IDictionary<,>))!;
            if (implementation != null)
                return BuildDictionaryComparer(type, implementation, typeof(ICollection<>));

            // IReadOnlyDictionary
            implementation = type.GetTargetImplementation(typeof(IReadOnlyDictionary<,>))!;
            if (implementation != null)
                return BuildDictionaryComparer(type, implementation, typeof(IReadOnlyCollection<>));

            // ICollection
            implementation = type.GetTargetImplementation(typeof(ICollection<>))!;
            if (implementation != null)
                return BuildCollectionComparer(type, implementation);

            // IReadOnlyCollection
            implementation = type.GetTargetImplementation(typeof(IReadOnlyCollection<>))!;
            if (implementation != null)
                return BuildCollectionComparer(type, implementation);

            // IEnumerable
            implementation = type.GetTargetImplementation(typeof(IEnumerable<>))!;
            if (implementation != null)
            {
                var elementType = implementation.GenericTypeArguments[0];
                var getCount = typeof(Enumerable).GetMethods()
                    .Single(x => x.Name == nameof(Enumerable.Count) && x.GetParameters().Length == 1)
                    .MakeGenericMethod(elementType);
                var getElement = GetElementAtProperty(elementType);

                return BuildSequentialCollectionComparer(
                    type,
                    elementType,
                    x => Expression.Call(null, getCount, x),
                    (x, i) => Expression.Call(null, getElement, x, i)
                );
            }

            return BuildPropertyFieldComparer(type);

            static bool IsEqualsMethod(MethodInfo method, Type valueType) =>
                method.IsPublic &&
                !method.IsStatic &&
                method.Name == nameof(Equals) &&
                method.GetParameters().Length == 1 &&
                method.GetParameters()[0].ParameterType == valueType;

            static LambdaExpression BuildDictionaryComparer(Type source, Type implementation, Type countTarget)
            {
                var (keyType, valueType, _) = implementation.GenericTypeArguments;
                var getCount = GetCountPropertyGetter(source.GetTargetImplementation(countTarget)!);
                var getKeys = source.GetProperty(nameof(IReadOnlyDictionary<object, object>.Keys))!.GetGetMethod()!;
                var containsKey = source.GetMethod(nameof(IReadOnlyDictionary<object, object>.ContainsKey))!;
                var getElement = GetIndexProperty(source, keyType);

                return BuildKeyedCollectionComparer(
                    source,
                    keyType,
                    valueType,
                    x => Expression.Call(x, getCount),
                    x => Expression.Call(x, getKeys),
                    (x, k) => Expression.Call(x, containsKey, k),
                    (x, k) => Expression.Property(x, getElement, k)
                );
            }

            static LambdaExpression BuildCollectionComparer(Type source, Type implementation)
            {
                var elementType = implementation.GenericTypeArguments[0];
                var getCount = GetCountPropertyGetter(implementation);
                var getElement = GetElementAtProperty(elementType);

                return BuildSequentialCollectionComparer(
                    source,
                    elementType,
                    x => Expression.Call(x, getCount),
                    (x, i) => Expression.Call(null, getElement, x, i)
                );
            }

            static MethodInfo GetCountPropertyGetter(Type collectionType) =>
                collectionType.GetProperty(nameof(ICollection<object>.Count))!.GetGetMethod()!;

            static PropertyInfo GetIndexProperty(Type collectionType, Type indexType) => collectionType.GetProperties()
                .Single(x => x.GetIndexParameters().Length == 1 && x.GetIndexParameters()[0].ParameterType == indexType);

            static MethodInfo GetElementAtProperty(Type elementType) => typeof(Enumerable)
                .GetMethod(nameof(Enumerable.ElementAt))!.MakeGenericMethod(elementType);
        }
    }
}