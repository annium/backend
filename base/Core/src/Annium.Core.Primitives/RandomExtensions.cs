using System;
using System.Linq;

namespace Annium.Core.Primitives
{
    public static class RandomExtensions
    {
        public static bool NextBool(this Random random) => random.Next(0, 1) == 1;

        public static T NextEnum<T>(this Random random) where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).OfType<T>().ToArray();

            return values[random.Next(0, values.Length)];
        }

        public static decimal NextDecimal(this Random random) => (decimal) random.NextDouble();
    }
}