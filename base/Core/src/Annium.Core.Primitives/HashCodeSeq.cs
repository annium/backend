using System.Collections.Generic;

namespace Annium.Core.Primitives
{
    public static class HashCodeSeq
    {
        public static int Combine<T>(IEnumerable<T> seq)
            where T : notnull
        {
            unchecked
            {
                var hash = 19;

                foreach (var x in seq)
                    hash = hash * 31 + x.GetHashCode();

                return hash;
            }
        }
    }
}