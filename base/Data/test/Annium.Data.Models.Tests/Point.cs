using System.Collections.Generic;

namespace Annium.Data.Models.Tests
{
    public class Point : Equatable<Point>
    {
        public int X { get; }

        public int Y { get; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override IEnumerable<int> GetComponentHashCodes()
        {
            yield return X.GetHashCode();
            yield return Y.GetHashCode();
        }
    }
}