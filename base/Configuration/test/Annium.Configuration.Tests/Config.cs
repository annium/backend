using System.Collections.Generic;
using Annium.Core.Application.Types;

namespace Annium.Configuration.Tests
{
    public class Config
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

    public class Val
    {
        public int Plain { get; set; }

        public decimal[] Array { get; set; }
    }

    public abstract class SomeConfig
    {
        [ResolveField]
        public string Type { get; set; }
    }

    [ResolveKey(nameof(ConfigOne))]
    public class ConfigOne : SomeConfig
    {
        public uint Value { get; set; }
    }

    [ResolveKey(nameof(ConfigTwo))]
    public class ConfigTwo : SomeConfig
    {
        public long Value { get; set; }
    }
}