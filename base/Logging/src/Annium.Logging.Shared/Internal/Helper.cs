using System;
using System.Collections.Generic;
using System.Text;

namespace Annium.Logging.Shared.Internal
{
    internal static class Helper
    {
        public static (string, IReadOnlyDictionary<string, object>) Process(string messageTemplate, object[] dataItems)
        {
            var keyIndex = -1;
            var itemIndex = -1;
            var sb = new StringBuilder();
            var data = new Dictionary<string, object>();

            for (var i = 0; i < messageTemplate.Length; i++)
            {
                var ch = messageTemplate[i];
                switch (ch)
                {
                    case '{':
                        // ensure not opening nested template var
                        if (keyIndex != -1)
                            throw new InvalidOperationException($"Nested template vars are not supported. Template: {messageTemplate}. Position: {i}.");
                        keyIndex = i;
                        break;
                    case '}':
                        if (keyIndex == -1)
                            throw new InvalidOperationException($"Template var already resolved. Template: {messageTemplate}. Position: {i}.");

                        var key = messageTemplate.AsSpan(keyIndex + 1, i - keyIndex - 1).ToString();
                        keyIndex = -1;

                        ++itemIndex;
                        if (dataItems.Length > itemIndex)
                            sb.Append(data[key] = dataItems[itemIndex]);
                        else
                            throw new InvalidOperationException($"Missing data item #{itemIndex}. Template: {messageTemplate}. Position: {i}.");
                        break;
                    default:
                        // if not inside template var name - just add char
                        if (keyIndex == -1)
                            sb.Append(ch);
                        break;
                }
            }

            var extra = dataItems.Length - itemIndex - 1;
            if (extra != 0)
                throw new InvalidOperationException($"Unexpected {extra} data item(s). Template: {messageTemplate}.");

            return (sb.ToString(), data);
        }
    }
}