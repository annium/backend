using System.Linq;
using Annium.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Collections.Tests.Generic.Extensions;

public class SortedListExtensionsTest
{
    [Fact]
    public void GetRange()
    {
        // arrange
        var data = Enumerable.Range(1, 5).ToSortedList(x => x);

        // act & assert - invalid range
        var span = data.GetRange(0, 2);
        span.IsDefault();

        // act & assert - move forward
        span = data.GetRange(1, 2)!;
        span.IsNotDefault();
        span.Count.Is(2);
        span.Move(-1).IsFalse();
        span[0].Is(new(1, 1));
        span[1].Is(new(2, 2));
        span.Move(3).IsTrue();
        span[0].Is(new(4, 4));
        span[1].Is(new(5, 5));

        // act & assert - move backward
        span = data.GetRange(4, 5)!;
        span.IsNotDefault();
        span.Count.Is(2);
        span.Move(1).IsFalse();
        span[0].Is(new(4, 4));
        span[1].Is(new(5, 5));
        span.Move(-3).IsTrue();
        span[0].Is(new(1, 1));
        span[1].Is(new(2, 2));
    }

    [Fact]
    public void GetChunks()
    {
        // arrange
        var data = Enumerable.Range(1, 5).ToSortedList(x => x);

        // act & assert - missing beginning
        var chunks = data.GetChunks(0, 2, Next);
        chunks.Has(2);
        chunks.At((0, 0)).IsDefault();
        chunks.At((1, 2)).Is(data.GetRange(1, 2));

        // act & assert - missing end
        chunks = data.GetChunks(3, 6, Next);
        chunks.Has(2);
        chunks.At((3, 5)).Is(data.GetRange(3, 5));
        chunks.At((6, 6)).IsDefault();

        // act & assert - missing parts
        data = new[] { 1, 3, 5 }.ToSortedList(x => x);
        chunks = data.GetChunks(0, 6, Next);
        chunks.Has(7);
        chunks.At((0, 0)).IsDefault();
        chunks.At((1, 1)).Is(data.GetRange(1, 1));
        chunks.At((2, 2)).IsDefault();
        chunks.At((3, 3)).Is(data.GetRange(3, 3));
        chunks.At((4, 4)).IsDefault();
        chunks.At((5, 5)).Is(data.GetRange(5, 5));
        chunks.At((6, 6)).IsDefault();

        // act & assert - chunk size control
        data = new[] { 1, 3, 4, 6 }.ToSortedList(x => x);
        chunks = data.GetChunks(0, 7, Next, 2);
        chunks.Has(3);
        chunks.At((0, 2)).IsDefault();
        chunks.At((3, 4)).Is(data.GetRange(3, 4));
        chunks.At((5, 7)).IsDefault();

        // act & assert - chunk size control - skip all chunks
        data = new[] { 1, 3, 4, 6 }.ToSortedList(x => x);
        chunks = data.GetChunks(0, 7, Next, 3);
        chunks.Has(1);
        chunks.At((0, 7)).IsDefault();

        // act & assert - chunk size control - resolve chunk if matches all data
        data = new[] { 1, 2, 3 }.ToSortedList(x => x);
        chunks = data.GetChunks(1, 3, Next, 4);
        chunks.Has(1);
        chunks.At((1, 3)).Is(data.GetRange(1, 3));

        static int Next(int x) => x + 1;
    }
}