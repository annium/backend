using System.Collections.Generic;
using Annium.Extensions.Mapper;

namespace Annium.Extensions.Configuration.Tests
{
    internal class Config
    {
        public bool Flag { get; set; }

        public int Plain { get; set; }

        public int[] Array { get; set; }

        public List<int[]> Matrix { get; set; }

        public List<Val> List { get; set; }

        public Dictionary<string, Val> Dictionary { get; set; }

        public Val Nested { get; set; }

        public SomeConfig Abstract { get; set; }
    }

    internal class Val
    {
        public int Plain { get; set; }

        public decimal[] Array { get; set; }
    }

    internal abstract class SomeConfig
    {
        [ResolveField]
        public string Type { get; set; }
    }

    [ResolveKey(nameof(ConfigOne))]
    internal class ConfigOne : SomeConfig
    {
        public uint Value { get; set; }
    }

    [ResolveKey(nameof(ConfigTwo))]
    internal class ConfigTwo : SomeConfig
    {
        public long Value { get; set; }
    }
}