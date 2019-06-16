using System.Collections.Generic;

namespace Annium.Extensions.Validation
{
    public static class BaseRuleExtensions
    {
        public static IRuleBuilder<TValue, TField> IsRequired<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            string message = null
        ) => rule.Add((context, value) =>
        {
            if (EqualityComparer<TField>.Default.Equals(value, default(TField)))
                context.Error(message ?? "Value is required");
        });
    }
}