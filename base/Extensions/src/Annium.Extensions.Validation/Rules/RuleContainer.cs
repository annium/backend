using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Extensions.Validation
{
    internal class RuleContainer<TValue, TField> : IRuleContainer<TValue>, IRuleBuilder<TValue, TField>
    {
        private readonly Func<TValue, TField> getField;

        private readonly IList<IRule<TValue, TField>> chain = new List<IRule<TValue, TField>>();

        public RuleContainer(
            Func<TValue, TField> getField
        )
        {
            this.getField = getField;
        }

        public IRuleBuilder<TValue, TField> Add(Action<ValidationContext<TValue>, TField> validate)
        {
            chain.Add(new SyncRule<TValue, TField>(validate));

            return this;
        }

        public IRuleBuilder<TValue, TField> Add(Func<ValidationContext<TValue>, TField, Task> validate)
        {
            chain.Add(new AsyncRule<TValue, TField>(validate));

            return this;
        }

        public IRuleBuilder<TValue, TField> Then()
        {
            chain.Add(new ThenRule<TValue, TField>());

            return this;
        }

        public async Task Validate(TValue value, ValidationContext<TValue> context)
        {
            var field = getField(value);

            foreach (var rule in chain)
            {
                // short-circuting rule in chain stops execution
                if (context.Result.HasErrors && rule.ShortCircuit)
                    return;

                switch (rule)
                {
                    case SyncRule<TValue, TField> syncRule:
                        syncRule.Validate.Invoke(context, field);
                        break;
                    case AsyncRule<TValue, TField> asyncRule:
                        await asyncRule.Validate.Invoke(context, field);
                        break;
                }
            }
        }
    }
}