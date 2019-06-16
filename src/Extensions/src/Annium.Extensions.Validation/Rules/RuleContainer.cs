using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Extensions.Validation
{
    internal class Rule<TValue, TField> : IRuleContainer<TValue>, IRuleBuilder<TValue, TField>
    {
        private readonly Func<TValue, TField> getField;

        private readonly IList<IRule<TValue, TField>> chain = new List<IRule<TValue, TField>>();

        public Rule(
            Func<TValue, TField> getField
        )
        {
            this.getField = getField;
        }

        public IRuleBuilder<TValue, TField> Add(Action<ValidationContext<TValue>, TField> validate)
        {
            chain.Add(new SyncRule<TValue, TField>(validate, false));

            return this;
        }

        public IRuleBuilder<TValue, TField> Add(Func<ValidationContext<TValue>, TField, Task> validate)
        {
            chain.Add(new AsyncRule<TValue, TField>(validate, false));

            return this;
        }

        public async Task<BooleanResult> Validate(TValue value, ValidationContext<TValue> context)
        {
            var field = getField(value);
            var result = Result.Success();

            foreach (var rule in chain)
            {
                switch (rule)
                {
                    case SyncRule<TValue, TField> syncRule:
                        syncRule.Validate.Invoke(context, field);
                        break;
                    case AsyncRule<TValue, TField> asyncRule:
                        await asyncRule.Validate.Invoke(context, field);
                        break;
                    default:
                        break;
                }
            }

            return result;
        }
    }
}