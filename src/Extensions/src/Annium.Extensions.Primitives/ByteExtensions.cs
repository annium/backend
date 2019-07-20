namespace Annium.Extensions.Primitives
{
    public static class ByteExtensions
    {
        private static readonly char[] hexLookup = CreateHexLookup();

        public static string ToHexString(this byte[] byteArray)
        {
            var lookup = hexLookup;
            var result = new char[byteArray.Length * 2];

            for (var i = 0; i < byteArray.Length; i++)
            {
                var value = byteArray[i];
                result[2 * i] = lookup[2 * value];
                result[2 * i + 1] = lookup[2 * value + 1];
            }

            return new string(result);
        }

        private static char[] CreateHexLookup()
        {
            var result = new char[512];

            for (int i = 0; i < 256; i++)
            {
                var s = i.ToString("X2");
                result[2 * i] = s[0];
                result[2 * i + 1] = s[1];
            }

            return result;
        }
    }
}