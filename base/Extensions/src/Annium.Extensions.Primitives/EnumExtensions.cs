using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Annium.Extensions.Primitives
{
    public static class EnumExtensions
    {
        public static T ParseEnum<T>(this string str)
            where T : struct, Enum
        {
            var (succeed, value) = str.TryParseEnum<T>();
            if (!succeed)
                throw new ArgumentException($"'{str}' is not a {typeof(T).Name} value");

            return value;
        }

        public static T ParseFlags<T>(this string str, char separator)
            where T : struct, Enum
        {
            var values = str.Split(separator)
                .Select(x => x.Trim())
                .Where(x => !x.IsNullOrWhiteSpace())
                .Select(x => x.ParseEnum<T>())
                .ToList();

            if (values.Count == 0)
                return (T) (ValueType) 0;

            var typeCode = default(T).GetTypeCode();

            return typeCode switch
            {
                TypeCode.Byte   => (T) (ValueType) values.Cast<byte>().Aggregate(0, (a, v) => a | v),
                TypeCode.UInt16 => (T) (ValueType) values.Cast<ushort>().Aggregate(0, (a, v) => a | v),
                TypeCode.Int32  => (T) (ValueType) values.Cast<int>().Aggregate(0, (a, v) => a | v),
                TypeCode.UInt32 => (T) (ValueType) values.Cast<uint>().Aggregate(0U, (a, v) => a | v),
                TypeCode.Int64  => (T) (ValueType) values.Cast<long>().Aggregate(0L, (a, v) => a | v),
                TypeCode.UInt64 => (T) (ValueType) values.Cast<ulong>().Aggregate(0UL, (a, v) => a | v),
                _               => throw new ArgumentException($"'{typeCode}' based Flags Enum is not supported"),
            };
        }

        public static T ParseEnum<T>(this string str, T defaultValue)
            where T : struct, Enum
        {
            var (succeed, value) = str.TryParseEnum<T>();

            return succeed ? value : defaultValue;
        }


        public static T ParseFlags<T>(this string str, char separator, T defaultValue)
            where T : struct, Enum
        {
            var values = str.Split(separator)
                .Select(x => x.Trim())
                .Where(x => !x.IsNullOrWhiteSpace())
                .Select(x => x.ParseEnum(defaultValue))
                .ToList();

            if (values.Count == 0)
                return defaultValue;

            var typeCode = default(T).GetTypeCode();

            return typeCode switch
            {
                TypeCode.Byte   => (T) (ValueType) values.Cast<byte>().Aggregate(0, (a, v) => a | v),
                TypeCode.UInt16 => (T) (ValueType) values.Cast<ushort>().Aggregate(0, (a, v) => a | v),
                TypeCode.Int32  => (T) (ValueType) values.Cast<int>().Aggregate(0, (a, v) => a | v),
                TypeCode.UInt32 => (T) (ValueType) values.Cast<uint>().Aggregate(0U, (a, v) => a | v),
                TypeCode.Int64  => (T) (ValueType) values.Cast<long>().Aggregate(0L, (a, v) => a | v),
                TypeCode.UInt64 => (T) (ValueType) values.Cast<ulong>().Aggregate(0UL, (a, v) => a | v),
                _               => throw new ArgumentException($"'{typeCode}' based Flags Enum is not supported"),
            };
        }

        private static (bool succeed, T value) TryParseEnum<T>(this string str)
            where T : struct, Enum
        {
            var enumType = Enum.GetUnderlyingType(typeof(T));

            foreach (var item in typeof(T).GetFields().Where(x => x.IsStatic))
            {
                if (item.Name.Equals(str, StringComparison.OrdinalIgnoreCase) ||
                    (item.GetCustomAttribute<DescriptionAttribute>()?.Description.Equals(str, StringComparison.OrdinalIgnoreCase) ?? false))
                    return (true, (T) item.GetValue(null)!);

                var value = (T) item.GetValue(null)!;
                if (Convert.ChangeType(value, enumType).ToString() == str)
                    return (true, value);
            }

            return (false, default);
        }
    }
}