using Annium.Core.Mapper.Attributes;

namespace Annium.Logging.Abstractions
{
    [AutoMapped]
    public enum LogLevel
    {
        Trace = 0,

        Debug = 1,

        Info = 2,

        Warn = 3,

        Error = 4,

        None = 5,
    }
}