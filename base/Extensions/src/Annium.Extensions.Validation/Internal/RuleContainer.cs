using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Extensions.Validation
{
    internal class RuleContainer<TValue, TField> : IRuleBuilder<TValue, TField>, IRuleContainer<TValue>
    {
        private readonly Func<TValue, TField> getField;
        private readonly IList<IList<Delegate>> chains = new List<IList<Delegate>>();

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
            chains[chains.Count - 1].Add(validate);

            return this;
        }

        public IRuleBuilder<TValue, TField> Add(Func<ValidationContext<TValue>, TField, Task> validate)
        {
            chains[chains.Count - 1].Add(validate);

            return this;
        }

        public IRuleBuilder<TValue, TField> Then()
        {
            chains.Add(new List<Delegate>());

            return this;
        }

        public async Task<bool> ValidateAsync(
            ValidationContext<TValue> context,
            TValue value,
            int stage
        )
        {
            // no validation if no chain at this stage
            if (stage >= chains.Count)
                return false;

            var field = getField(value);

            foreach (var rule in chains[stage])
                switch (rule)
                {
                    case Action<ValidationContext<TValue>, TField> validate:
                        validate(context, field);
                        break;
                    case Func<ValidationContext<TValue>, TField, Task> validate:
                        await validate(context, field);
                        break;
                }

            return true;
        }
    }
}