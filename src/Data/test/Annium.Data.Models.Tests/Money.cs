using System;
using System.Collections.Generic;

namespace Annium.Data.Models.Tests
{
    public class Money : Comparable<Money>
    {
        public int Major { get; }

        public int Minor { get; }

        public Money(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }

        public override int GetHashCode()
        {
            var hash = 7;

            hash = hash * 31 + Major.GetHashCode();
            hash = hash * 31 + Minor.GetHashCode();

            return hash;
        }

        protected override IEnumerable<Func<Money, IComparable>> GetComparables()
        {
            yield return x => x.Major;
            yield return x => x.Minor;
        }
    }
}