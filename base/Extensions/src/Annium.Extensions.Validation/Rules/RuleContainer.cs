using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Extensions.Validation
{
    internal class RuleContainer<TValue, TField> : IRuleBuilder<TValue, TField>, IRuleContainer<TValue>
    {
        public int StageCount => chains.Count;
        private readonly Func<TValue, TField> getField;
        private readonly IList<IList<IRule<TValue, TField>>> chains = new List<IList<IRule<TValue, TField>>>();

        public RuleContainer(
            Func<TValue, TField> getField
        )
        {
            this.getField = getField;

            // call Then to init stage
            Then();
        }

        public IRuleBuilder<TValue, TField> Add(Action<ValidationContext<TValue>, TField> validate)
        {
            chains[chains.Count - 1].Add(new SyncRule<TValue, TField>(validate));

            return this;
        }

        public IRuleBuilder<TValue, TField> Add(Func<ValidationContext<TValue>, TField, Task> validate)
        {
            chains[chains.Count - 1].Add(new AsyncRule<TValue, TField>(validate));

            return this;
        }

        public IRuleBuilder<TValue, TField> Then()
        {
            chains.Add(new List<IRule<TValue, TField>>());

            return this;
        }

        public async Task ValidateAsync(
            ValidationContext<TValue> context,
            TValue value,
            int stage
        )
        {
            // no validation if no chain at this stage
            if (stage >= chains.Count)
                return;

            var field = getField(value);

            foreach (var rule in chains[stage])
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