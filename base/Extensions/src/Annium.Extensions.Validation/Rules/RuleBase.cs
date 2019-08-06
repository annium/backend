namespace Annium.Extensions.Validation
{
    internal abstract class RuleBase<TValue, TField> : IRule<TValue, TField>
    {
        public bool ShortCircuit { get; }

        protected RuleBase(bool shortCircuit)
        {
            ShortCircuit = shortCircuit;
        }
    }
}