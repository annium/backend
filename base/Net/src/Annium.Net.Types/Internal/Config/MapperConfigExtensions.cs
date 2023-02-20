using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Annium.Net.Types.Refs;
using NodaTime;

namespace Annium.Net.Types.Internal.Config;

internal static class MapperConfigExtensions
{
    public static IMapperConfig RegisterBaseTypes(this IMapperConfig cfg) => cfg
        .RegisterBaseType(typeof(object), BaseType.Object)
        .RegisterBaseType(typeof(bool), BaseType.Bool)
        .RegisterBaseType(typeof(string), BaseType.String)
        .RegisterBaseType(typeof(byte), BaseType.Byte)
        .RegisterBaseType(typeof(sbyte), BaseType.SByte)
        .RegisterBaseType(typeof(int), BaseType.Int)
        .RegisterBaseType(typeof(uint), BaseType.UInt)
        .RegisterBaseType(typeof(long), BaseType.Long)
        .RegisterBaseType(typeof(ulong), BaseType.ULong)
        .RegisterBaseType(typeof(float), BaseType.Float)
        .RegisterBaseType(typeof(double), BaseType.Double)
        .RegisterBaseType(typeof(decimal), BaseType.Decimal)
        .RegisterBaseType(typeof(Guid), BaseType.Guid)
        .RegisterBaseType(typeof(DateTime), BaseType.DateTime)
        .RegisterBaseType(typeof(DateTimeOffset), BaseType.DateTimeOffset)
        .RegisterBaseType(typeof(DateOnly), BaseType.Date)
        .RegisterBaseType(typeof(TimeOnly), BaseType.Time)
        .RegisterBaseType(typeof(TimeSpan), BaseType.TimeSpan)
        .RegisterBaseType(typeof(Instant), BaseType.DateTimeOffset)
        .RegisterBaseType(typeof(LocalDate), BaseType.Date)
        .RegisterBaseType(typeof(LocalTime), BaseType.Time)
        .RegisterBaseType(typeof(LocalDateTime), BaseType.DateTime)
        .RegisterBaseType(typeof(ZonedDateTime), BaseType.DateTimeOffset)
        .RegisterBaseType(typeof(Duration), BaseType.TimeSpan)
        .RegisterBaseType(typeof(Period), BaseType.TimeSpan)
        .RegisterBaseType(typeof(YearMonth), BaseType.YearMonth)
        .RegisterBaseType(typeof(void), BaseType.Void);

    public static IMapperConfig RegisterIgnored(this IMapperConfig cfg) => cfg
        // basic types
        .RegisterIgnored(Match.Is(typeof(object)))
        .RegisterIgnored(Match.Is(typeof(ValueType)))
        // enumerable interfaces
        .RegisterIgnored(Match.Is(typeof(IEnumerable<>)))
        .RegisterIgnored(Match.Is(typeof(ICollection<>)))
        .RegisterIgnored(Match.Is(typeof(IReadOnlyCollection<>)))
        // dictionary interfaces
        .RegisterIgnored(Match.Is(typeof(IReadOnlyDictionary<,>)))
        .RegisterIgnored(Match.Is(typeof(IDictionary<,>)))
        // base type interfaces
        .RegisterIgnored(Match.Is(typeof(IComparable<>)))
        .RegisterIgnored(Match.Is(typeof(IEquatable<>)))
        // low-level interfaces
        .RegisterIgnored(Match.Is(typeof(ISpanParsable<>)))
        .RegisterIgnored(Match.Is(typeof(IParsable<>)))
        // tasks
        .RegisterIgnored(Match.IsDerivedFrom(typeof(Task), self: true))
        .RegisterIgnored(Match.IsDerivedFrom(typeof(Task<>), self: true))
        .RegisterIgnored(Match.Is(typeof(ValueTask)))
        .RegisterIgnored(Match.Is(typeof(ValueTask<>)))
        // custom basic interfaces
        .RegisterIgnored(Match.Is(typeof(ICopyable<>)));

    public static IMapperConfig RegisterArrays(this IMapperConfig cfg) => cfg
        .RegisterArray(typeof(IEnumerable<>))
        .RegisterArray(typeof(IReadOnlyCollection<>))
        .RegisterArray(typeof(ICollection<>))
        .RegisterArray(typeof(IReadOnlyList<>))
        .RegisterArray(typeof(IList<>))
        .RegisterArray(typeof(IReadOnlySet<>))
        .RegisterArray(typeof(ISet<>))
        .RegisterArray(typeof(List<>))
        .RegisterArray(typeof(HashSet<>));

    public static IMapperConfig RegisterRecords(this IMapperConfig cfg) => cfg
        .RegisterRecord(typeof(IDictionary<,>))
        .RegisterRecord(typeof(IReadOnlyDictionary<,>))
        .RegisterRecord(typeof(Dictionary<,>))
        .RegisterRecord(typeof(ImmutableDictionary<,>));
}