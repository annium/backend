using System.Collections.Generic;
using Annium.Core.Mapper.Attributes;
using Annium.Core.Runtime.Types;

namespace Annium.Configuration.Tests
{
    public class Config
    {
        public bool Flag { get; set; }
        public int Plain { get; set; }
        public int[] Array { get; set; } = System.Array.Empty<int>();
        public List<int[]> Matrix { get; set; } = new();
        public List<Val> List { get; set; } = new();
        public Dictionary<string, Val> Dictionary { get; set; } = new();
        public Val Nested { get; set; } = new();

        public SomeConfig Abstract { get; set; } = new ConfigOne();
        public SomeEnum Enum { get; set; }
    }

    public class Val
    {
        public int Plain { get; set; }

        public decimal[] Array { get; set; } = System.Array.Empty<decimal>();
    }

    public abstract class SomeConfig
    {
        [ResolutionKey]
        public string Type { get; protected set; } = string.Empty;
    }

    [ResolutionKeyValue(nameof(ConfigOne))]
    public class ConfigOne : SomeConfig
    {
        public uint Value { get; set; }

        public ConfigOne()
        {
            Type = nameof(ConfigOne);
        }
    }

    [ResolutionKeyValue(nameof(ConfigTwo))]
    public class ConfigTwo : SomeConfig
    {
        public long Value { get; set; }

        public ConfigTwo()
        {
            Type = nameof(ConfigTwo);
        }
    }

    [AutoMapped]
    public enum SomeEnum
    {
        One,
        Two
    }
}