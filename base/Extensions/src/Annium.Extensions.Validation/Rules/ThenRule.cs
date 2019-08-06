namespace Annium.Extensions.Validation
{
    internal class ThenRule<TValue, TField> : RuleBase<TValue, TField>
    {
        public ThenRule() : base(shortCircuit: true) { }
    }
}