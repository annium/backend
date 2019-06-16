using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Validation
{
    internal class SyncRule<TValue, TField> : RuleBase<TValue, TField>
    {
        public Action<ValidationContext<TValue>, TField> Validate { get; }

        public SyncRule(
            Action<ValidationContext<TValue>, TField> validate,
            bool shortCircuit
        ) : base(shortCircuit)
        {
            Validate = validate;
        }
    }

    internal class AsyncRule<TValue, TField> : RuleBase<TValue, TField>
    {
        public Func<ValidationContext<TValue>, TField, Task> Validate { get; }

        public AsyncRule(
            Func<ValidationContext<TValue>, TField, Task> validate,
            bool shortCircuit
        ) : base(shortCircuit)
        {
            Validate = validate;
        }
    }

    internal abstract class RuleBase<TValue, TField> : IRule<TValue, TField>
    {
        public bool ShortCircuit { get; }

        protected RuleBase(bool shortCircuit)
        {
            ShortCircuit = shortCircuit;
        }
    }
}