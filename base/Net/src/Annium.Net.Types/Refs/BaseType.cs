using System;

namespace Annium.Net.Types.Refs;

public static class BaseType
{
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
    public const string Promise = "promise";

    internal static BaseTypeRef? GetRefFor(Type type) => MapperConfig.GetBaseTypeRefFor(type);
}