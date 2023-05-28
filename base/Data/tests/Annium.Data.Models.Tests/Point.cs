using System;

namespace Annium.Data.Models.Tests;

public class Point : Equatable<Point>
{
    public int X { get; }

    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);
}