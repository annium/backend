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
}