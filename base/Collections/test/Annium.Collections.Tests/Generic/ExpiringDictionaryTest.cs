using System;
using System.Collections.Generic;
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
    public class ExpiringDictionaryTest
    {
        [Fact]
        public void Add_Works()
        {
            // arrange
            var timeProvider = GetTimeProvider();
            var collection = new ExpiringDictionary<int, string>(timeProvider);
            var ttl = Duration.FromSeconds(5);

            // act
            Parallel.ForEach(Enumerable.Range(0, 100), (x, _, _) => collection.Add(x, $"val:{x}", ttl));

            // assert
            foreach (var value in Enumerable.Range(0, 100))
                collection.ContainsKey(value).IsTrue();
        }

        [Fact]
        public void Get_Works()
        {
            // arrange
            var timeProvider = GetTimeProvider();
            var collection = new ExpiringDictionary<Guid, string>(timeProvider);
            var key = Guid.NewGuid();
            var value = "secret";
            var ttl = Duration.FromSeconds(5);
            collection.Add(key, value, ttl);

            // assert
            collection.Get(key).Is(value);
            timeProvider.SetNow(timeProvider.Now + ttl);
            collection.Get(key).Is(value);
            timeProvider.SetNow(timeProvider.Now + ttl + Duration.FromMilliseconds(1));
            ((Func<string>) (() => collection.Get(key))).Throws<KeyNotFoundException>();
        }

        [Fact]
        public void TryGet_Works()
        {
            // arrange
            var timeProvider = GetTimeProvider();
            var collection = new ExpiringDictionary<Guid, string>(timeProvider);
            var key = Guid.NewGuid();
            var value = "secret";
            var ttl = Duration.FromSeconds(5);
            collection.Add(key, value, ttl);

            // assert
            var val = string.Empty;
            collection.TryGet(key, out val).IsTrue();
            val.Is(value);
            timeProvider.SetNow(timeProvider.Now + ttl);
            collection.Get(key).Is(value);
            timeProvider.SetNow(timeProvider.Now + ttl + Duration.FromMilliseconds(1));
            ((Func<string>) (() => collection.Get(key))).Throws<KeyNotFoundException>();
        }

        [Fact]
        public void ContainsKey_Works()
        {
            // arrange
            var timeProvider = GetTimeProvider();
            var collection = new ExpiringDictionary<Guid, string>(timeProvider);
            var key = Guid.NewGuid();
            var ttl = Duration.FromSeconds(5);
            collection.Add(key, "secret", ttl);

            // assert
            collection.ContainsKey(key).IsTrue();
            timeProvider.SetNow(timeProvider.Now + ttl);
            collection.ContainsKey(key).IsTrue();
            timeProvider.SetNow(timeProvider.Now + ttl + Duration.FromMilliseconds(1));
            collection.ContainsKey(key).IsFalse();
        }

        [Fact]
        public void Remove_Works()
        {
            // arrange
            var timeProvider = GetTimeProvider();
            var collection = new ExpiringDictionary<Guid, string>(timeProvider);
            var key1 = Guid.NewGuid();
            var key2 = Guid.NewGuid();
            var ttl = Duration.FromSeconds(5);
            collection.Add(key1, "a", ttl);
            collection.Add(key2, "b", ttl * 2);

            // assert
            collection.Remove(key2, out _).IsTrue();
            collection.ContainsKey(key1).IsTrue();
            collection.ContainsKey(key2).IsFalse();
            timeProvider.SetNow(timeProvider.Now + ttl + Duration.FromMilliseconds(1));
            collection.Remove(key2, out _).IsFalse();
            collection.ContainsKey(key1).IsFalse();
            collection.ContainsKey(key2).IsFalse();
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