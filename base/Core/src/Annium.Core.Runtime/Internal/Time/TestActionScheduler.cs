using System;
using Annium.Core.Primitives;
using Annium.Core.Runtime.Time;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time
{
    internal class TestActionScheduler : IActionScheduler
    {
        private readonly IManagedTimeProvider _timeProvider;

        public TestActionScheduler(
            IManagedTimeProvider timeProvider
        )
        {
            _timeProvider = timeProvider;
        }

        public Action Delay(Action handle, int timeout)
            => Delay(handle, Duration.FromMilliseconds(timeout));

        public Action Delay(Action handle, Duration timeout)
        {
            var lasting = Duration.Zero;

            _timeProvider.NowChanged += CheckTime;

            void CheckTime(Duration duration)
            {
                lasting += duration;
                if (lasting < timeout)
                    return;
                _timeProvider.NowChanged -= CheckTime;
                handle();
            }

            return () => _timeProvider.NowChanged -= CheckTime;
        }

        public Action Interval(Action handle, int interval)
            => Interval(handle, Duration.FromMilliseconds(interval));

        public Action Interval(Action handle, Duration interval)
        {
            var lasting = Duration.Zero;

            _timeProvider.NowChanged += CheckTime;

            void CheckTime(Duration duration)
            {
                lasting += duration;
                if (lasting < interval)
                    return;

                lasting -= interval;
                handle();
            }

            return () => _timeProvider.NowChanged -= CheckTime;
        }
    }
}