using System.Collections.Generic;

namespace Annium.Extensions.Validation
{
    public static class BaseRuleExtensions
    {
        public static IRule<T> IsRequired<T>(this IRule<T> rule, string message = null) where T : class =>
            rule.Add(v =>
            {
                if (EqualityComparer<T>.Default.Equals(v, default(T)))
                    return message ?? "Value is required";

                return null;
            });
    }
}