using System;

namespace Annium.Serialization.Abstractions
{
    internal class InternalSerializer<TSource, TDestination> : ISerializer<TSource, TDestination>
    {
        private readonly Func<TSource, TDestination> serialize;
        private readonly Func<TDestination, TSource> deserialize;

        public InternalSerializer(
            Func<TSource, TDestination> serialize,
            Func<TDestination, TSource> deserialize
        )
        {
            this.deserialize = deserialize;
            this.serialize = serialize;
        }

        public TSource Deserialize(TDestination value) => deserialize(value);

        public TDestination Serialize(TSource value) => serialize(value);
    }

    internal class InternalSerializer<TValue> : ISerializer<TValue>
    {
        private readonly Func<object, TValue> serialize;
        private readonly Func<Type, TValue, object> deserialize;

        public InternalSerializer(
            Func<object, TValue> serialize,
            Func<Type, TValue, object> deserialize
        )
        {
            this.serialize = serialize;
            this.deserialize = deserialize;
        }

        public T Deserialize<T>(TValue value) => (T) deserialize(typeof(T), value);

        public object Deserialize(Type type, TValue value) => deserialize(type, value);

        public TValue Serialize<T>(T value) => serialize(value!);

        public TValue Serialize(object value) => serialize(value);
    }
}