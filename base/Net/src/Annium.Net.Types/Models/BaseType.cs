using System;
using System.Collections.Generic;
using Annium.Core.Primitives;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Internal.Models;
using NodaTime;

namespace Annium.Net.Types.Models;

public static class BaseType
{
    private static readonly Dictionary<Type, StructModel> BaseTypes = new();
    public const string Object = "object";
    public const string Bool = "bool";
    public const string String = "string";
    public const string Byte = "byte";
    public const string SByte = "sbyte";
    public const string Int = "int";
    public const string UInt = "uint";
    public const string Long = "long";
    public const string ULong = "ulong";
    public const string Decimal = "decimal";
    public const string Guid = "guid";
    public const string DateTime = "dateTime";
    public const string DateTimeOffset = "dateTimeOffset";
    public const string Date = "date";
    public const string Time = "time";
    public const string TimeSpan = "timeSpan";
    public const string YearMonth = "yearMonth";
    public const string Void = "void";

    static BaseType()
    {
        Register<object>(Object);
        Register<bool>(Bool);
        Register<string>(String);
        Register<byte>(Byte);
        Register<sbyte>(SByte);
        Register<int>(Int);
        Register<uint>(UInt);
        Register<long>(Long);
        Register<ulong>(ULong);
        Register<decimal>(Decimal);
        Register<Guid>(Guid);
        Register<DateTime>(DateTime);
        Register<DateTimeOffset>(DateTimeOffset);
        Register<DateOnly>(Date);
        Register<TimeOnly>(Time);
        Register<TimeSpan>(TimeSpan);
        Register<Instant>(DateTimeOffset);
        Register<LocalDate>(Date);
        Register<LocalTime>(Time);
        Register<LocalDateTime>(DateTime);
        Register<ZonedDateTime>(DateTimeOffset);
        Register<Duration>(TimeSpan);
        Register<Period>(TimeSpan);
        Register<YearMonth>(YearMonth);
        Register(typeof(void), Void);
    }

    public static StructModel? GetFor(Type type) => BaseTypes.GetValueOrDefault(type);

    public static void Register<T>(string name) => Register(typeof(T), name);

    public static void Register(Type type, string name)
    {
        if (type is { IsClass: false, IsValueType: false })
            throw new ArgumentException($"Type {type.FriendlyName()} is neither class nor struct");

        if (type.IsGenericType || type.IsGenericTypeDefinition)
            throw new ArgumentException($"Type {type.FriendlyName()} is generic type");

        if (type.IsGenericTypeParameter)
            throw new ArgumentException($"Type {type.FriendlyName()} is generic type parameter");

        if (!BaseTypes.TryAdd(type, StructModelBuilder.Init(type.GetNamespace(), name).Build()))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered");
    }
}