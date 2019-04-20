using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Annium.Testing.Elements;

namespace Annium.Testing
{
    public static class FilterExtensions
    {
        public static IEnumerable<Test> FilterMask(this IEnumerable<Test> tests, string mask)
        {
            if (string.IsNullOrWhiteSpace(mask))
                return tests;

            var pattern = Regex.Escape(mask).Replace(@"\*", ".*").Replace(@"\?", ".");
            var regex = new Regex($"^{pattern}$", RegexOptions.IgnoreCase);

            return tests.Where(t => regex.IsMatch(t.DisplayName)).ToArray();
        }
    }
}