using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Validation
{
    public static class ComplexRuleExtensions
    {
        public static IRuleBuilder<TValue, TField> Unique<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            Func<TField, Task<bool>> getEntityPresenceAsync,
            string message = null
        ) => rule.Add(async(context, value) =>
        {
            var exists = await getEntityPresenceAsync(value);
            if (exists)
                context.Error(message ?? "{0} with {1} {2} already exists", typeof(TValue).Name, context.Field, value);
        });

        public static IRuleBuilder<TValue, TField> Unique<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            Func<TField, bool> getEntityPresence,
            string message = null
        ) => rule.Add((context, value) =>
        {
            var exists = getEntityPresence(value);
            if (exists)
                context.Error(message ?? "{0} with {1} {2} already exists", typeof(TValue).Name, context.Field, value);
        });
    }
}