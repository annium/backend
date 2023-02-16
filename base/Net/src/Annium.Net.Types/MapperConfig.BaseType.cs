using System;
using System.Collections.Generic;
using Annium.Core.Primitives;
using Annium.Net.Types.Refs;
using NodaTime;

namespace Annium.Net.Types;

public static partial class MapperConfig
{
    private static readonly Dictionary<Type, BaseTypeRef> BaseTypes = new();

    private static void RegisterBaseTypes()
    {
        RegisterBaseType<object>(BaseType.Object);
        RegisterBaseType<bool>(BaseType.Bool);
        RegisterBaseType<string>(BaseType.String);
        RegisterBaseType<byte>(BaseType.Byte);
        RegisterBaseType<sbyte>(BaseType.SByte);
        RegisterBaseType<int>(BaseType.Int);
        RegisterBaseType<uint>(BaseType.UInt);
        RegisterBaseType<long>(BaseType.Long);
        RegisterBaseType<ulong>(BaseType.ULong);
        RegisterBaseType<float>(BaseType.Float);
        RegisterBaseType<double>(BaseType.Double);
        RegisterBaseType<decimal>(BaseType.Decimal);
        RegisterBaseType<Guid>(BaseType.Guid);
        RegisterBaseType<DateTime>(BaseType.DateTime);
        RegisterBaseType<DateTimeOffset>(BaseType.DateTimeOffset);
        RegisterBaseType<DateOnly>(BaseType.Date);
        RegisterBaseType<TimeOnly>(BaseType.Time);
        RegisterBaseType<TimeSpan>(BaseType.TimeSpan);
        RegisterBaseType<Instant>(BaseType.DateTimeOffset);
        RegisterBaseType<LocalDate>(BaseType.Date);
        RegisterBaseType<LocalTime>(BaseType.Time);
        RegisterBaseType<LocalDateTime>(BaseType.DateTime);
        RegisterBaseType<ZonedDateTime>(BaseType.DateTimeOffset);
        RegisterBaseType<Duration>(BaseType.TimeSpan);
        RegisterBaseType<Period>(BaseType.TimeSpan);
        RegisterBaseType<YearMonth>(BaseType.YearMonth);
        RegisterBaseType(typeof(void), BaseType.Void);
    }

    public static void RegisterBaseType<T>(string name) => RegisterBaseType(typeof(T), name);

    public static void RegisterBaseType(Type type, string name)
    {
        if (type is { IsClass: false, IsValueType: false })
            throw new ArgumentException($"Type {type.FriendlyName()} is neither class nor struct");

        if (type.IsGenericType || type.IsGenericTypeDefinition)
            throw new ArgumentException($"Type {type.FriendlyName()} is generic type");

        if (type.IsGenericTypeParameter)
            throw new ArgumentException($"Type {type.FriendlyName()} is generic type parameter");

        if (!BaseTypes.TryAdd(type, new BaseTypeRef(name)))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered");
    }

    internal static BaseTypeRef? GetBaseTypeRefFor(Type type) => BaseTypes.GetValueOrDefault(type);
}