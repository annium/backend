using System;
using System.Runtime.CompilerServices;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Tests
{
    public class TrackingWeakReferenceTest
    {
        [Fact]
        public void TrackingWeakReference_Works()
        {
            // arrange
            var counter = 0;
            object target;
            TrackingWeakReference<object> reference = default!;
            Wrap(() =>
            {
                target = new object();
                reference = new TrackingWeakReference<object>(target, () => counter++);
            });

            // act
            target = default!;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // assert
            reference.IsAlive.IsFalse();

            // act
            reference = default!;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // assert
            counter.IsEqual(1);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Wrap(Action wrap) => wrap();
    }
}