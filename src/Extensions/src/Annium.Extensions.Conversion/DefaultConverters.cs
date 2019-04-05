using System;
using static Annium.Extensions.Conversion.Converter;

namespace Annium.Extensions.Conversion
{
    internal static class DefaultConverters
    {
        public static void Register()
        {
            Register<string, string>(e => e);

            // string -> xxx
            Register<string, bool>(bool.Parse);
            Register<string, int>(int.Parse);
            Register<string, uint>(uint.Parse);
            Register<string, long>(long.Parse);
            Register<string, ulong>(ulong.Parse);
            Register<string, float>(float.Parse);
            Register<string, double>(double.Parse);
            Register<string, decimal>(decimal.Parse);
            Register<string, DateTime>(DateTime.Parse);
            Register<string, DateTimeOffset>(DateTimeOffset.Parse);
            Register<string, Guid>(Guid.Parse);
            Register<string, Uri>(e => new Uri(e));

            // xxx -> string
            Register<bool, string>(e => e.ToString());
            Register<int, string>(e => e.ToString());
            Register<uint, string>(e => e.ToString());
            Register<long, string>(e => e.ToString());
            Register<ulong, string>(e => e.ToString());
            Register<float, string>(e => e.ToString());
            Register<double, string>(e => e.ToString());
            Register<decimal, string>(e => e.ToString());
            Register<DateTime, string>(e => e.ToString());
            Register<DateTimeOffset, string>(e => e.ToString());
            Register<Guid, string>(e => e.ToString());
            Register<Uri, string>(e => e.ToString());
        }
    }
}