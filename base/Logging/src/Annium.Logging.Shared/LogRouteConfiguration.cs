using System;

namespace Annium.Logging.Shared
{
    public class LogRouteConfiguration
    {
        public TimeSpan BufferTime { get; }
        public int BufferCount { get; }

        public LogRouteConfiguration(TimeSpan bufferTime, int bufferCount)
        {
            BufferTime = bufferTime;
            BufferCount = bufferCount;
        }

        public LogRouteConfiguration()
        {
            BufferTime = TimeSpan.Zero;
            BufferCount = 0;
        }
    }
}