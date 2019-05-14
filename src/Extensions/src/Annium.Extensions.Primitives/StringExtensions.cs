using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Extensions.Primitives
{
    public static class StringExtensions
    {
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

        private enum Symbol
        {
            Upper,
            Lower,
            Digit,
            Other,
        }
    }
}