namespace Annium.Extensions.Validation
{
    internal interface IRule<TValue, TField>
    {
        bool ShortCircuit { get; }
    }
}