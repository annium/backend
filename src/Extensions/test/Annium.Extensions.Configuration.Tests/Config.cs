using System.Collections.Generic;

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
    }

    internal class Val
    {
        public int Plain { get; set; }

        public decimal[] Array { get; set; }
    }
}