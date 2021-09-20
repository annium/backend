using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators
{
    public class WhenCompletedTest
    {
        [Fact]
        public async Task SubscribeAsync_OnErrorWorksCorrectly()
        {
            // arrange
            var log = new List<long>();

            // act
            await Observable.Interval(TimeSpan.FromMilliseconds(20)).Take(5).Do(log.Add).WhenCompleted();

            log.IsEqual(new[] { 0, 1, 2, 3, 4 });
        }
    }
}