using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Time;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;

namespace Annium.Collections.Tests.Generic
{
    public class ExpiringCollectionTest
    {
        [Fact]
        public void Add_Works()
        {
            // arrange
            var timeProvider = GetTimeProvider();
            var collection = new ExpiringCollection<int>(timeProvider);
            var ttl = Duration.FromSeconds(5);

            // act
            Parallel.ForEach(Enumerable.Range(0, 100), (x, _, _) => collection.Add(x, ttl));

            // assert
            foreach (var value in Enumerable.Range(0, 100))
                collection.Contains(value).IsTrue();
        }

        [Fact]
        public void Contains_Works()
        {
            // arrange
            var timeProvider = GetTimeProvider();
            var collection = new ExpiringCollection<Guid>(timeProvider);
            var value = Guid.NewGuid();
            var ttl = Duration.FromSeconds(5);
            collection.Add(value, ttl);

            // assert
            collection.Contains(value).IsTrue();
            timeProvider.SetNow(timeProvider.Now + ttl);
            collection.Contains(value).IsTrue();
            timeProvider.SetNow(timeProvider.Now + ttl + Duration.FromMilliseconds(1));
            collection.Contains(value).IsFalse();
        }

        [Fact]
        public void Remove_Works()
        {
            // arrange
            var timeProvider = GetTimeProvider();
            var collection = new ExpiringCollection<Guid>(timeProvider);
            var value1 = Guid.NewGuid();
            var value2 = Guid.NewGuid();
            var ttl = Duration.FromSeconds(5);
            collection.Add(value1, ttl);
            collection.Add(value2, ttl * 2);

            // assert
            collection.Remove(value2).IsTrue();
            collection.Contains(value1).IsTrue();
            collection.Contains(value2).IsFalse();
            timeProvider.SetNow(timeProvider.Now + ttl + Duration.FromMilliseconds(1));
            collection.Remove(value2).IsFalse();
            collection.Contains(value1).IsFalse();
            collection.Contains(value2).IsFalse();
        }

        private IManagedTimeProvider GetTimeProvider()
        {
            var container = new ServiceContainer();
            container.AddTestTimeProvider();
            var provider = container.BuildServiceProvider();

            var timeProvider = provider.GetRequiredService<IManagedTimeProvider>();
            timeProvider.SetNow(Instant.MinValue);

            return timeProvider;
        }
    }
}