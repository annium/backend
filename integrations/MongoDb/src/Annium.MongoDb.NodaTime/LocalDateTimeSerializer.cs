﻿using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

public class LocalDateTimeSerializer : PatternSerializer<LocalDateTime>
{
    public LocalDateTimeSerializer()
        : base(LocalDateTimePattern.ExtendedIso, d => d.WithCalendar(CalendarSystem.Iso)) { }
}
