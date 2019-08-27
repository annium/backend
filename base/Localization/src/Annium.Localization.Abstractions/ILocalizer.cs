namespace Annium.Localization.Abstractions
{
    public interface ILocalizer<T> : ILocalizer
    {

    }

    public interface ILocalizer
    {
        string this[string entry] { get; }
    }
}