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

        public override int GetHashCode()
        {
            var hash = 7;

            hash = hash * 31 + X.GetHashCode();
            hash = hash * 31 + Y.GetHashCode();

            return hash;
        }
    }
}