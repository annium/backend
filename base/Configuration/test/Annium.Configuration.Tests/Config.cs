using System.Collections.Generic;
using Annium.Core.Runtime;
using Annium.Core.Runtime.Types;

namespace Annium.Configuration.Tests
{
    public class Config
    {
        public bool Flag { get; set; }

        public int Plain { get; set; }

        public int[] Array { get; set; } = System.Array.Empty<int>();

        public List<int[]> Matrix { get; set; } = new List<int[]>();

        public List<Val> List { get; set; } = new List<Val>();

        public Dictionary<string, Val> Dictionary { get; set; } = new Dictionary<string, Val>();

        public Val Nested { get; set; } = new Val();

        public SomeConfig Abstract { get; set; } = new ConfigOne();
    }

    public class Val
    {
        public int Plain { get; set; }

        public decimal[] Array { get; set; } = System.Array.Empty<decimal>();
    }

    public abstract class SomeConfig
    {
        [ResolveField]
        public string Type { get; protected set; } = string.Empty;
    }

    [ResolveKey(nameof(ConfigOne))]
    public class ConfigOne : SomeConfig
    {
        public uint Value { get; set; }
        public ConfigOne()
        {
            Type = nameof(ConfigOne);
        }
    }

    [ResolveKey(nameof(ConfigTwo))]
    public class ConfigTwo : SomeConfig
    {
        public long Value { get; set; }
        public ConfigTwo()
        {
            Type = nameof(ConfigTwo);
        }
    }
}