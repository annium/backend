namespace Annium.Extensions.Arguments
{
    internal interface IArgumentProcessor
    {
        RawConfiguration Compose(string[] args);
    }
}