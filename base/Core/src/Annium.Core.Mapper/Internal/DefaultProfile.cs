using System;
using System.Globalization;
using NodaTime;

namespace Annium.Core.Mapper.Internal
{
    internal class DefaultProfile : Profile
    {
        public DefaultProfile()
        {
            RegisterString();
            RegisterByte();
            RegisterSbyte();
            RegisterChar();
            RegisterDecimal();
            RegisterDouble();
            RegisterFloat();
            RegisterInt();
            RegisterUint();
            RegisterLong();
            RegisterUlong();
            RegisterShort();
            RegisterUshort();
            RegisterNodaTime();
        }

        private void RegisterString()
        {
            // from
            Map<string, bool>(x => bool.Parse(x));
            Map<string, int>(x => int.Parse(x));
            Map<string, uint>(x => uint.Parse(x));
            Map<string, long>(x => long.Parse(x));
            Map<string, ulong>(x => ulong.Parse(x));
            Map<string, float>(x => float.Parse(x));
            Map<string, double>(x => double.Parse(x));
            Map<string, decimal>(x => decimal.Parse(x));
            Map<string, DateTime>(x => DateTime.Parse(x));
            Map<string, DateTimeOffset>(x => DateTimeOffset.Parse(x));
            Map<string, Guid>(x => Guid.Parse(x));
            Map<string, Uri>(x => new Uri(x));

            // to
            Map<bool, string>(x => x.ToString());
            Map<int, string>(x => x.ToString());
            Map<uint, string>(x => x.ToString());
            Map<long, string>(x => x.ToString());
            Map<ulong, string>(x => x.ToString());
            Map<float, string>(x => x.ToString(CultureInfo.CurrentUICulture));
            Map<double, string>(x => x.ToString(CultureInfo.CurrentUICulture));
            Map<decimal, string>(x => x.ToString(CultureInfo.CurrentUICulture));
            Map<DateTime, string>(x => x.ToString(CultureInfo.CurrentUICulture));
            Map<DateTimeOffset, string>(x => x.ToString());
            Map<Guid, string>(x => x.ToString());
            Map<Uri, string>(x => x.ToString());
        }

        private void RegisterByte()
        {
            //from
            Map<byte, sbyte>(x => (sbyte) x);
            Map<byte, char>(x => (char) x);
            Map<byte, decimal>(x => (decimal) x);
            Map<byte, double>(x => (double) x);
            Map<byte, float>(x => (float) x);
            Map<byte, int>(x => (int) x);
            Map<byte, uint>(x => (uint) x);
            Map<byte, long>(x => (long) x);
            Map<byte, ulong>(x => (ulong) x);
            Map<byte, short>(x => (short) x);
            Map<byte, ushort>(x => (ushort) x);

            //to
            Map<sbyte, byte>(x => (byte) x);
            Map<char, byte>(x => (byte) x);
            Map<decimal, byte>(x => (byte) x);
            Map<double, byte>(x => (byte) x);
            Map<float, byte>(x => (byte) x);
            Map<int, byte>(x => (byte) x);
            Map<uint, byte>(x => (byte) x);
            Map<long, byte>(x => (byte) x);
            Map<ulong, byte>(x => (byte) x);
            Map<short, byte>(x => (byte) x);
            Map<ushort, byte>(x => (byte) x);
        }

        private void RegisterSbyte()
        {
            //from
            Map<sbyte, byte>(x => (byte) x);
            Map<sbyte, char>(x => (char) x);
            Map<sbyte, decimal>(x => (decimal) x);
            Map<sbyte, double>(x => (double) x);
            Map<sbyte, float>(x => (float) x);
            Map<sbyte, int>(x => (int) x);
            Map<sbyte, uint>(x => (uint) x);
            Map<sbyte, long>(x => (long) x);
            Map<sbyte, ulong>(x => (ulong) x);
            Map<sbyte, short>(x => (short) x);
            Map<sbyte, ushort>(x => (ushort) x);

            //to
            Map<byte, sbyte>(x => (sbyte) x);
            Map<char, sbyte>(x => (sbyte) x);
            Map<decimal, sbyte>(x => (sbyte) x);
            Map<double, sbyte>(x => (sbyte) x);
            Map<float, sbyte>(x => (sbyte) x);
            Map<int, sbyte>(x => (sbyte) x);
            Map<uint, sbyte>(x => (sbyte) x);
            Map<long, sbyte>(x => (sbyte) x);
            Map<ulong, sbyte>(x => (sbyte) x);
            Map<short, sbyte>(x => (sbyte) x);
            Map<ushort, sbyte>(x => (sbyte) x);
        }

        private void RegisterChar()
        {
            //from
            Map<char, byte>(x => (byte) x);
            Map<char, sbyte>(x => (sbyte) x);
            Map<char, decimal>(x => (decimal) x);
            Map<char, double>(x => (double) x);
            Map<char, float>(x => (float) x);
            Map<char, int>(x => (int) x);
            Map<char, uint>(x => (uint) x);
            Map<char, long>(x => (long) x);
            Map<char, ulong>(x => (ulong) x);
            Map<char, short>(x => (short) x);
            Map<char, ushort>(x => (ushort) x);

            //to
            Map<byte, char>(x => (char) x);
            Map<sbyte, char>(x => (char) x);
            Map<decimal, char>(x => (char) x);
            Map<double, char>(x => (char) x);
            Map<float, char>(x => (char) x);
            Map<int, char>(x => (char) x);
            Map<uint, char>(x => (char) x);
            Map<long, char>(x => (char) x);
            Map<ulong, char>(x => (char) x);
            Map<short, char>(x => (char) x);
            Map<ushort, char>(x => (char) x);
        }

        private void RegisterDecimal()
        {
            //from
            Map<decimal, byte>(x => (byte) x);
            Map<decimal, sbyte>(x => (sbyte) x);
            Map<decimal, char>(x => (char) x);
            Map<decimal, double>(x => (double) x);
            Map<decimal, float>(x => (float) x);
            Map<decimal, int>(x => (int) x);
            Map<decimal, uint>(x => (uint) x);
            Map<decimal, long>(x => (long) x);
            Map<decimal, ulong>(x => (ulong) x);
            Map<decimal, short>(x => (short) x);
            Map<decimal, ushort>(x => (ushort) x);

            //to
            Map<byte, decimal>(x => (decimal) x);
            Map<sbyte, decimal>(x => (decimal) x);
            Map<char, decimal>(x => (decimal) x);
            Map<double, decimal>(x => (decimal) x);
            Map<float, decimal>(x => (decimal) x);
            Map<int, decimal>(x => (decimal) x);
            Map<uint, decimal>(x => (decimal) x);
            Map<long, decimal>(x => (decimal) x);
            Map<ulong, decimal>(x => (decimal) x);
            Map<short, decimal>(x => (decimal) x);
            Map<ushort, decimal>(x => (decimal) x);
        }

        private void RegisterDouble()
        {
            //from
            Map<double, byte>(x => (byte) x);
            Map<double, sbyte>(x => (sbyte) x);
            Map<double, char>(x => (char) x);
            Map<double, decimal>(x => (decimal) x);
            Map<double, float>(x => (float) x);
            Map<double, int>(x => (int) x);
            Map<double, uint>(x => (uint) x);
            Map<double, long>(x => (long) x);
            Map<double, ulong>(x => (ulong) x);
            Map<double, short>(x => (short) x);
            Map<double, ushort>(x => (ushort) x);

            //to
            Map<byte, double>(x => (double) x);
            Map<sbyte, double>(x => (double) x);
            Map<char, double>(x => (double) x);
            Map<decimal, double>(x => (double) x);
            Map<float, double>(x => (double) x);
            Map<int, double>(x => (double) x);
            Map<uint, double>(x => (double) x);
            Map<long, double>(x => (double) x);
            Map<ulong, double>(x => (double) x);
            Map<short, double>(x => (double) x);
            Map<ushort, double>(x => (double) x);
        }

        private void RegisterFloat()
        {
            //from
            Map<float, byte>(x => (byte) x);
            Map<float, sbyte>(x => (sbyte) x);
            Map<float, char>(x => (char) x);
            Map<float, decimal>(x => (decimal) x);
            Map<float, double>(x => (double) x);
            Map<float, int>(x => (int) x);
            Map<float, uint>(x => (uint) x);
            Map<float, long>(x => (long) x);
            Map<float, ulong>(x => (ulong) x);
            Map<float, short>(x => (short) x);
            Map<float, ushort>(x => (ushort) x);

            //to
            Map<byte, float>(x => (float) x);
            Map<sbyte, float>(x => (float) x);
            Map<char, float>(x => (float) x);
            Map<decimal, float>(x => (float) x);
            Map<double, float>(x => (float) x);
            Map<int, float>(x => (float) x);
            Map<uint, float>(x => (float) x);
            Map<long, float>(x => (float) x);
            Map<ulong, float>(x => (float) x);
            Map<short, float>(x => (float) x);
            Map<ushort, float>(x => (float) x);
        }

        private void RegisterInt()
        {
            //from
            Map<int, byte>(x => (byte) x);
            Map<int, sbyte>(x => (sbyte) x);
            Map<int, char>(x => (char) x);
            Map<int, decimal>(x => (decimal) x);
            Map<int, double>(x => (double) x);
            Map<int, float>(x => (float) x);
            Map<int, uint>(x => (uint) x);
            Map<int, long>(x => (long) x);
            Map<int, ulong>(x => (ulong) x);
            Map<int, short>(x => (short) x);
            Map<int, ushort>(x => (ushort) x);

            //to
            Map<byte, int>(x => (int) x);
            Map<sbyte, int>(x => (int) x);
            Map<char, int>(x => (int) x);
            Map<decimal, int>(x => (int) x);
            Map<double, int>(x => (int) x);
            Map<float, int>(x => (int) x);
            Map<uint, int>(x => (int) x);
            Map<long, int>(x => (int) x);
            Map<ulong, int>(x => (int) x);
            Map<short, int>(x => (int) x);
            Map<ushort, int>(x => (int) x);
        }

        private void RegisterUint()
        {
            //from
            Map<uint, byte>(x => (byte) x);
            Map<uint, sbyte>(x => (sbyte) x);
            Map<uint, char>(x => (char) x);
            Map<uint, decimal>(x => (decimal) x);
            Map<uint, double>(x => (double) x);
            Map<uint, float>(x => (float) x);
            Map<uint, int>(x => (int) x);
            Map<uint, long>(x => (long) x);
            Map<uint, ulong>(x => (ulong) x);
            Map<uint, short>(x => (short) x);
            Map<uint, ushort>(x => (ushort) x);

            //to
            Map<byte, uint>(x => (uint) x);
            Map<sbyte, uint>(x => (uint) x);
            Map<char, uint>(x => (uint) x);
            Map<decimal, uint>(x => (uint) x);
            Map<double, uint>(x => (uint) x);
            Map<float, uint>(x => (uint) x);
            Map<int, uint>(x => (uint) x);
            Map<long, uint>(x => (uint) x);
            Map<ulong, uint>(x => (uint) x);
            Map<short, uint>(x => (uint) x);
            Map<ushort, uint>(x => (uint) x);
        }

        private void RegisterLong()
        {
            //from
            Map<long, byte>(x => (byte) x);
            Map<long, sbyte>(x => (sbyte) x);
            Map<long, char>(x => (char) x);
            Map<long, decimal>(x => (decimal) x);
            Map<long, double>(x => (double) x);
            Map<long, float>(x => (float) x);
            Map<long, int>(x => (int) x);
            Map<long, uint>(x => (uint) x);
            Map<long, ulong>(x => (ulong) x);
            Map<long, short>(x => (short) x);
            Map<long, ushort>(x => (ushort) x);

            //to
            Map<byte, long>(x => (long) x);
            Map<sbyte, long>(x => (long) x);
            Map<char, long>(x => (long) x);
            Map<decimal, long>(x => (long) x);
            Map<double, long>(x => (long) x);
            Map<float, long>(x => (long) x);
            Map<int, long>(x => (long) x);
            Map<uint, long>(x => (long) x);
            Map<ulong, long>(x => (long) x);
            Map<short, long>(x => (long) x);
            Map<ushort, long>(x => (long) x);
        }

        private void RegisterUlong()
        {
            //from
            Map<ulong, byte>(x => (byte) x);
            Map<ulong, sbyte>(x => (sbyte) x);
            Map<ulong, char>(x => (char) x);
            Map<ulong, decimal>(x => (decimal) x);
            Map<ulong, double>(x => (double) x);
            Map<ulong, float>(x => (float) x);
            Map<ulong, int>(x => (int) x);
            Map<ulong, uint>(x => (uint) x);
            Map<ulong, long>(x => (long) x);
            Map<ulong, short>(x => (short) x);
            Map<ulong, ushort>(x => (ushort) x);

            //to
            Map<byte, ulong>(x => (ulong) x);
            Map<sbyte, ulong>(x => (ulong) x);
            Map<char, ulong>(x => (ulong) x);
            Map<decimal, ulong>(x => (ulong) x);
            Map<double, ulong>(x => (ulong) x);
            Map<float, ulong>(x => (ulong) x);
            Map<int, ulong>(x => (ulong) x);
            Map<uint, ulong>(x => (ulong) x);
            Map<long, ulong>(x => (ulong) x);
            Map<short, ulong>(x => (ulong) x);
            Map<ushort, ulong>(x => (ulong) x);
        }

        private void RegisterShort()
        {
            //from
            Map<short, byte>(x => (byte) x);
            Map<short, sbyte>(x => (sbyte) x);
            Map<short, char>(x => (char) x);
            Map<short, decimal>(x => (decimal) x);
            Map<short, double>(x => (double) x);
            Map<short, float>(x => (float) x);
            Map<short, int>(x => (int) x);
            Map<short, uint>(x => (uint) x);
            Map<short, long>(x => (long) x);
            Map<short, ulong>(x => (ulong) x);
            Map<short, ushort>(x => (ushort) x);

            //to
            Map<byte, short>(x => (short) x);
            Map<sbyte, short>(x => (short) x);
            Map<char, short>(x => (short) x);
            Map<decimal, short>(x => (short) x);
            Map<double, short>(x => (short) x);
            Map<float, short>(x => (short) x);
            Map<int, short>(x => (short) x);
            Map<uint, short>(x => (short) x);
            Map<long, short>(x => (short) x);
            Map<ulong, short>(x => (short) x);
            Map<ushort, short>(x => (short) x);
        }

        private void RegisterUshort()
        {
            //from
            Map<ushort, byte>(x => (byte) x);
            Map<ushort, sbyte>(x => (sbyte) x);
            Map<ushort, char>(x => (char) x);
            Map<ushort, decimal>(x => (decimal) x);
            Map<ushort, double>(x => (double) x);
            Map<ushort, float>(x => (float) x);
            Map<ushort, int>(x => (int) x);
            Map<ushort, uint>(x => (uint) x);
            Map<ushort, long>(x => (long) x);
            Map<ushort, ulong>(x => (ulong) x);
            Map<ushort, short>(x => (short) x);

            //to
            Map<byte, ushort>(x => (ushort) x);
            Map<sbyte, ushort>(x => (ushort) x);
            Map<char, ushort>(x => (ushort) x);
            Map<decimal, ushort>(x => (ushort) x);
            Map<double, ushort>(x => (ushort) x);
            Map<float, ushort>(x => (ushort) x);
            Map<int, ushort>(x => (ushort) x);
            Map<uint, ushort>(x => (ushort) x);
            Map<long, ushort>(x => (ushort) x);
            Map<ulong, ushort>(x => (ushort) x);
            Map<short, ushort>(x => (ushort) x);
        }

        private void RegisterNodaTime()
        {
            Map<DateTime, Instant>(d => Instant.FromDateTimeUtc(d.ToUniversalTime()));
            Map<Instant, DateTime>(i => i.ToDateTimeUtc());
            Map<DateTimeOffset, Instant>(d => Instant.FromDateTimeOffset(d));
            Map<Instant, DateTimeOffset>(i => i.ToDateTimeOffset());
        }
    }
}