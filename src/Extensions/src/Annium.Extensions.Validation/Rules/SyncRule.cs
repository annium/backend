using System;

namespace Annium.Extensions.Validation
{
    internal class SyncRule<TValue, TField> : RuleBase<TValue, TField>
    {
        public Action<ValidationContext<TValue>, TField> Validate { get; }

        public SyncRule(
            Action<ValidationContext<TValue>, TField> validate
        ) : base(shortCircuit: false)
        {
            Validate = validate;
        }
    }
}