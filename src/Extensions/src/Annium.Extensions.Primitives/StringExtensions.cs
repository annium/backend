using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Extensions.Primitives
{
    public static class StringExtensions
    {
        private static readonly IReadOnlyDictionary<char, byte> hexLookup = CreateHexLookup();

        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        public static string UpperFirst(this string value)
        {
            value = Preprocess(value);
            if (string.IsNullOrEmpty(value)) return value;

            return value.Substring(0, 1).ToUpperInvariant() + value.Substring(1);
        }

        public static string LowerFirst(this string value)
        {
            value = Preprocess(value);
            if (string.IsNullOrEmpty(value)) return value;

            return value.Substring(0, 1).ToLowerInvariant() + value.Substring(1);
        }

        public static string PascalCase(this string value)
        {
            return Compound(value, pascalCase);

            string pascalCase(string result, string word) =>
                result + word.ToLowerInvariant().UpperFirst();
        }

        public static string CamelCase(this string value)
        {
            return Compound(value, camelCase);

            string camelCase(string result, string word) =>
                result + (result == string.Empty ? word.ToLowerInvariant().LowerFirst() : word.ToLowerInvariant().UpperFirst());
        }

        public static string KebabCase(this string value)
        {
            return Compound(value, kebabCase);

            string kebabCase(string result, string word) =>
                result + (result == string.Empty ? string.Empty : "-") + word.ToLowerInvariant();
        }

        public static string SnakeCase(this string value)
        {
            return Compound(value, snakeCase);

            string snakeCase(string result, string word) =>
                result + (result == string.Empty ? string.Empty : "_") + word.ToLowerInvariant();
        }

        public static IEnumerable<string> ToWords(this string value)
        {
            value = Preprocess(value);
            if (string.IsNullOrEmpty(value)) yield break;

            var pos = Symbol.Other;
            var from = 0;
            for (var i = 0; i < value.Length; i++)
            {
                var s = GetSymbol(value[i]);

                switch (pos)
                {
                    case Symbol.Upper:
                        // if current is upper and next is lower - end here
                        if (s == Symbol.Upper)
                        {
                            if ((value.Length - i) > 1 && GetSymbol(value[i + 1]) == Symbol.Lower)
                                yield return end(i, s);
                        }
                        else if (s == Symbol.Lower || s == Symbol.Digit)
                            pos = s;
                        else
                            yield return end(i, s);
                        break;
                    case Symbol.Lower:
                        if (s == Symbol.Upper || s == Symbol.Other)
                            yield return end(i, s);
                        else if (s == Symbol.Digit)
                            pos = Symbol.Digit;
                        break;
                    case Symbol.Digit:
                        if (s != Symbol.Digit)
                            yield return end(i, s);
                        break;
                    default:
                        // check if we can enter word
                        if (s == Symbol.Upper || s == Symbol.Lower || s == Symbol.Digit)
                        {
                            from = i;
                            pos = s;
                        }
                        break;
                }
            }

            if (pos != Symbol.Other)
                yield return end(value.Length, Symbol.Other);

            string end(int i, Symbol s)
            {
                pos = s;
                var result = value.Substring(from, i - from);
                from = i;

                return result;
            }
        }

        public static byte[] FromHexStringToByteArray(this string str)
        {
            if (str is null)
                throw new ArgumentNullException(nameof(str));

            if (str.Length % 2 != 0)
                throw new FormatException("Hex string must contain even chars count");

            var lookup = hexLookup;
            var byteArray = new byte[str.Length / 2];
            for (var i = 0; i < str.Length; i += 2)
            {
                var c1 = str[i];
                var c2 = str[i + 1];

                if (!lookup.TryGetValue(c1, out var b1))
                    throw new OverflowException($"{c1} is not a valid hex character");

                if (!lookup.TryGetValue(c2, out var b2))
                    throw new OverflowException($"{c2} is not a valid hex character");

                byteArray[i / 2] = (byte) ((b1 << 4) + b2);
            }

            return byteArray;
        }

        public static bool TryFromHexStringToByteArray(this string str, out byte[] byteArray)
        {
            byteArray = null;

            if (str is null || str.Length % 2 != 0)
                return false;

            var lookup = hexLookup;
            var array = new byte[str.Length / 2];
            for (var i = 0; i < str.Length; i += 2)
            {
                var c1 = str[i];
                var c2 = str[i + 1];

                if (lookup.TryGetValue(c1, out var b1) && lookup.TryGetValue(c2, out var b2))
                    array[i / 2] = (byte) ((b1 << 4) + b2);
                else
                    return false;
            }

            byteArray = array;

            return true;
        }

        private static string Compound(string value, Func<string, string, string> callback)
        {
            value = Preprocess(value);
            if (string.IsNullOrEmpty(value)) return value;

            return ToWords(value).Aggregate(string.Empty, callback);
        }

        private static string Preprocess(string value) => value?.Trim();

        private static Symbol GetSymbol(char c)
        {
            if (char.IsUpper(c)) return Symbol.Upper;
            if (char.IsLower(c)) return Symbol.Lower;
            if (char.IsDigit(c)) return Symbol.Digit;
            return Symbol.Other;
        }

        private static IReadOnlyDictionary<char, byte> CreateHexLookup()
        {
            var result = new Dictionary<char, byte>();

            result['0'] = 0;
            result['1'] = 1;
            result['2'] = 2;
            result['3'] = 3;
            result['4'] = 4;
            result['5'] = 5;
            result['6'] = 6;
            result['7'] = 7;
            result['8'] = 8;
            result['9'] = 9;
            result['A'] = 10;
            result['B'] = 11;
            result['C'] = 12;
            result['D'] = 13;
            result['E'] = 14;
            result['F'] = 15;
            result['a'] = 10;
            result['b'] = 11;
            result['c'] = 12;
            result['d'] = 13;
            result['e'] = 14;
            result['f'] = 15;

            return result;
        }

        private enum Symbol
        {
            Upper,
            Lower,
            Digit,
            Other,
        }
    }
}