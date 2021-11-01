using NodaTime;

namespace Annium.NodaTime.Extensions
{
    public static class LocalDateTimeExtensions
    {
        public static bool IsMidnight(this LocalDateTime dateTime)
        {
            return dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.Second == 0;
        }
    }
}