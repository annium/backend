using System;

namespace Annium.Serialization.Abstractions
{
    public static class Serializer
    {
        public static ISerializer<TValue> Create<TValue>(
            Func<object, TValue> serialize,
            Func<Type, TValue, object> deserialize
        ) => new InternalSerializer<TValue>(serialize, deserialize);

        public static ISerializer<TSource, TDestination> Create<TSource, TDestination>(
            Func<TSource, TDestination> serialize,
            Func<TDestination, TSource> deserialize
        ) => new InternalSerializer<TSource, TDestination>(serialize, deserialize);
    }
}