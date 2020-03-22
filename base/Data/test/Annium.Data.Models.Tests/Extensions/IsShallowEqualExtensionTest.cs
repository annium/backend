using System;
using System.Collections.Generic;
using Annium.Data.Models.Extensions;
using Annium.Data.Models.Tests.Extensions.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Models.Tests.Extensions
{
    public class IsShallowEqualExtensionTest
    {
        [Fact]
        public void IsShallowEqual_Complex_Works()
        {
            // arrange
            var now = DateTimeOffset.Now;
            var demo1 = new Demo
            {
                Nullable = true,
                Uris = new[] { new Uri("http://localhost") },
                Samples = new List<Sample> { new Sample { Date = now, Point = new Point(1, 2) } },
                Points = new HashSet<Point>(new[] { new Point(1, 2) }),
                Dictionary = new Dictionary<Point, Sample> { { new Point(1, 2), new Sample { Date = now, Point = new Point(2, 1) } } },
                ReadOnlyDictionary = new Dictionary<Point, Sample> { { new Point(1, 2), new Sample { Date = now, Point = new Point(2, 1) } } },
            };
            var demo2 = new Demo
            {
                Nullable = true,
                Uris = new[] { new Uri("http://localhost") },
                Samples = new List<Sample> { new Sample { Date = now, Point = new Point(1, 2) } },
                Points = new HashSet<Point>(new[] { new Point(1, 2) }),
                Dictionary = new Dictionary<Point, Sample> { { new Point(1, 2), new Sample { Date = now, Point = new Point(2, 1) } } },
                ReadOnlyDictionary = new Dictionary<Point, Sample> { { new Point(1, 2), new Sample { Date = now, Point = new Point(2, 1) } } },
            };

            // assert
            demo1.IsShallowEqual(demo2).IsTrue();
        }

        [Fact]
        public void IsShallowEqual_Property_Works()
        {
            // arrange
            var now = DateTimeOffset.Now;
            Sample a = new Sample { Date = now, Point = new Point(2, 1) };
            Sample b = new Sample { Date = now, Point = new Point(2, 1) };
            Sample c = new Sample { Date = now + TimeSpan.FromSeconds(1), Point = new Point(2, 1) };
            Sample d = default;
            Sample e = a;

            // assert
            a.IsShallowEqual(b).IsTrue();
            a.IsShallowEqual(c).IsFalse();
            a.IsShallowEqual(d).IsFalse();
            a.IsShallowEqual(e).IsTrue();
        }

        [Fact]
        public void IsShallowEqual_Array_Works()
        {
            // arrange
            var now = DateTimeOffset.Now;
            Sample[] a = { new Sample { Date = now, Point = new Point(2, 1) } };
            Sample[] b = { new Sample { Date = now, Point = new Point(2, 1) } };
            Sample[] c = { new Sample { Date = now + TimeSpan.FromSeconds(1), Point = new Point(2, 1) } };
            Sample[] d = { default };
            Sample[] e = a;

            // assert
            a.IsShallowEqual(b).IsTrue();
            a.IsShallowEqual(c).IsFalse();
            a.IsShallowEqual(d).IsFalse();
            a.IsShallowEqual(e).IsTrue();
        }

        [Fact]
        public void IsShallowEqual_List_Works()
        {
            // arrange
            var now = DateTimeOffset.Now;
            IList<Sample> a = new List<Sample> { new Sample { Date = now, Point = new Point(2, 1) } };
            IList<Sample> b = new List<Sample> { new Sample { Date = now, Point = new Point(2, 1) } };
            IList<Sample> c = new List<Sample> { new Sample { Date = now + TimeSpan.FromSeconds(1), Point = new Point(2, 1) } };
            IList<Sample> d = new List<Sample> { default };
            IList<Sample> e = a;

            // assert
            a.IsShallowEqual(b).IsTrue();
            a.IsShallowEqual(c).IsFalse();
            a.IsShallowEqual(d).IsFalse();
            a.IsShallowEqual(e).IsTrue();
        }

        [Fact]
        public void IsShallowEqual_IDictionary_Works()
        {
            // arrange
            var now = DateTimeOffset.Now;
            IDictionary<Key, Sample> a = new Dictionary<Key, Sample> { { new Key(1, 1), new Sample { Date = now, Point = new Point(2, 1) } } };
            IDictionary<Key, Sample> b = new Dictionary<Key, Sample> { { new Key(1, 1), new Sample { Date = now, Point = new Point(2, 1) } } };
            IDictionary<Key, Sample> c = new Dictionary<Key, Sample> { { new Key(1, 2), new Sample { Date = now, Point = new Point(2, 1) } } };
            IDictionary<Key, Sample> d = new Dictionary<Key, Sample> { { new Key(0, 0), default } };
            IDictionary<Key, Sample> e = a;

            // assert
            a.IsShallowEqual(b).IsTrue();
            a.IsShallowEqual(c).IsFalse();
            a.IsShallowEqual(d).IsFalse();
            a.IsShallowEqual(e).IsTrue();
        }

        [Fact]
        public void IsShallowEqual_IReadOnlyDictionary_Works()
        {
            // arrange
            var now = DateTimeOffset.Now;
            IReadOnlyDictionary<Key, Sample> a = new Dictionary<Key, Sample> { { new Key(1, 1), new Sample { Date = now, Point = new Point(2, 1) } } };
            IReadOnlyDictionary<Key, Sample> b = new Dictionary<Key, Sample> { { new Key(1, 1), new Sample { Date = now, Point = new Point(2, 1) } } };
            IReadOnlyDictionary<Key, Sample> c = new Dictionary<Key, Sample> { { new Key(1, 2), new Sample { Date = now, Point = new Point(2, 1) } } };
            IReadOnlyDictionary<Key, Sample> d = new Dictionary<Key, Sample> { { new Key(0, 0), default } };
            IReadOnlyDictionary<Key, Sample> e = a;

            // assert
            a.IsShallowEqual(b).IsTrue();
            a.IsShallowEqual(c).IsFalse();
            a.IsShallowEqual(d).IsFalse();
            a.IsShallowEqual(e).IsTrue();
        }
    }

    namespace Internal
    {
        internal class Demo
        {
            public bool? Nullable { get; set; }

            public IEnumerable<Uri> Uris { get; set; }
            public List<Sample> Samples { get; set; }

            public HashSet<Point> Points { get; set; }
            public IDictionary<Point, Sample> Dictionary { get; set; }
            public IReadOnlyDictionary<Point, Sample> ReadOnlyDictionary { get; set; }
        }

        internal struct Sample
        {
            public DateTimeOffset Date { get; set; }
            public Point Point { get; set; }
        }

        internal class Key
        {
            public int X { get; }
            public int Y { get; }

            public Key(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
    }
}