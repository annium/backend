using Annium.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Collections.Tests.Generic;

public class IndexedSpanTests
{
    [Fact]
    public void Add_Count_Index_Move()
    {
        // arrange
        var data = new[] { 1m, 2m, 3m, 4m, 5m };

        // act & assert - move forward
        var span = data.ToReadOnlyIndexedSpan(0, 2);
        span.Count.Is(2);
        span.Move(-1).IsFalse();
        span[0].Is(1m);
        span[1].Is(2m);
        span.Move(3).IsTrue();
        span[0].Is(4m);
        span[1].Is(5m);

        // act & assert - move backward
        span = data.ToReadOnlyIndexedSpan(3, 2);
        span.Count.Is(2);
        span.Move(1).IsFalse();
        span[0].Is(4m);
        span[1].Is(5m);
        span.Move(-3).IsTrue();
        span[0].Is(1m);
        span[1].Is(2m);
    }
}