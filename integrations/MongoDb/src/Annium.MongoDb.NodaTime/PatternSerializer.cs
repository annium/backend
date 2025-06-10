using System;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

/// <summary>
/// Abstract base class for BSON serializers that use NodaTime patterns for serialization
/// </summary>
/// <typeparam name="TValue">The NodaTime type to serialize</typeparam>
public abstract class PatternSerializer<TValue> : SerializerBase<TValue>
{
    /// <summary>
    /// The NodaTime pattern used for serialization and deserialization
    /// </summary>
    private readonly IPattern<TValue> _pattern;

    /// <summary>
    /// Function to convert values before serialization, defaults to identity function
    /// </summary>
    private readonly Func<TValue, TValue> _valueConverter = v => v;

    /// <summary>
    /// Initializes a new instance of the PatternSerializer class with a pattern and value converter
    /// </summary>
    /// <param name="pattern">The NodaTime pattern to use for serialization</param>
    /// <param name="valueConverter">Function to convert values before serialization</param>
    protected PatternSerializer(IPattern<TValue> pattern, Func<TValue, TValue> valueConverter)
        : this(pattern)
    {
        _valueConverter = valueConverter;
    }

    /// <summary>
    /// Initializes a new instance of the PatternSerializer class with a pattern
    /// </summary>
    /// <param name="pattern">The NodaTime pattern to use for serialization</param>
    protected PatternSerializer(IPattern<TValue> pattern)
    {
        _pattern = pattern;
    }

    /// <summary>
    /// Deserializes a BSON value to a NodaTime value using the configured pattern
    /// </summary>
    /// <param name="context">The deserialization context</param>
    /// <param name="args">The deserialization arguments</param>
    /// <returns>The deserialized NodaTime value</returns>
    public override TValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.GetCurrentBsonType();
        switch (type)
        {
            case BsonType.String:
                return _valueConverter(_pattern.CheckedParse(context.Reader.ReadString()));
            case BsonType.Null:
                if (typeof(TValue).GetTypeInfo().IsValueType)
                    throw new InvalidOperationException(
                        $"{typeof(TValue).Name} is a value type, but the BsonValue is null."
                    );

                context.Reader.ReadNull();

                return default!;
            default:
                throw new NotSupportedException($"Cannot convert a {type} to a {typeof(TValue).Name}.");
        }
    }

    /// <summary>
    /// Serializes a NodaTime value to a BSON string using the configured pattern
    /// </summary>
    /// <param name="context">The serialization context</param>
    /// <param name="args">The serialization arguments</param>
    /// <param name="value">The NodaTime value to serialize</param>
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValue value)
    {
        context.Writer.WriteString(_pattern.Format(_valueConverter(value)));
    }
}
