using System.Collections.Generic;

namespace Annium.Extensions.Arguments
{
    internal class RawConfiguration
    {
        public IEnumerable<string> Positions { get; }

        public IEnumerable<string> Flags { get; }

        public IReadOnlyDictionary<string, string> Options { get; }

        public IReadOnlyDictionary<string, IEnumerable<string>> MultiOptions { get; }

        public string Raw { get; }

        public RawConfiguration(
            IEnumerable<string> positions,
            IEnumerable<string> flags,
            IReadOnlyDictionary<string, string> options,
            IReadOnlyDictionary<string, IEnumerable<string>> multiOptions,
            string raw
        )
        {
            Positions = positions;
            Flags = flags;
            Options = options;
            MultiOptions = multiOptions;
            Raw = raw;
        }
    }
}