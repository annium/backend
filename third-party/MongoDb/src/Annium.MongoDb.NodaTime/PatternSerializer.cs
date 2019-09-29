﻿using System;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime
{
    public abstract class PatternSerializer<TValue> : SerializerBase<TValue>
    {
        private readonly IPattern<TValue> pattern;

        private readonly Func<TValue, TValue> valueConverter = (v => v);

        protected PatternSerializer(
            IPattern<TValue> pattern,
            Func<TValue, TValue> valueConverter
        ) : this(pattern)
        {
            this.valueConverter = valueConverter ?? (v => v);
        }

        protected PatternSerializer(
            IPattern<TValue> pattern
        )
        {
            this.pattern = pattern;
        }

        public override TValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var type = context.Reader.GetCurrentBsonType();
            switch (type)
            {
                case BsonType.String:
                    return valueConverter(pattern.CheckedParse(context.Reader.ReadString()));
                case BsonType.Null:
                    if (typeof(TValue).GetTypeInfo().IsValueType)
                        throw new InvalidOperationException($"{typeof(TValue).Name} is a value type, but the BsonValue is null.");

                    context.Reader.ReadNull();

                    return default !;
                default:
                    throw new NotSupportedException($"Cannot convert a {type} to a {typeof(TValue).Name}.");
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValue value)
        {
            context.Writer.WriteString(this.pattern.Format(this.valueConverter(value)));
        }
    }
}