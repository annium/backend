using System;

namespace Annium.Extensions.Validation
{
    internal class SyncRule<TValue, TField> : IRule<TValue, TField>
    {
        public Action<ValidationContext<TValue>, TField> Validate { get; }

        public SyncRule(
            Action<ValidationContext<TValue>, TField> validate
        )
        {
            Validate = validate;
        }
    }
}