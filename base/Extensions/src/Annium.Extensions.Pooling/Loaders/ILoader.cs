namespace Annium.Extensions.Pooling.Loaders
{
    internal interface ILoader<T>
    {
        T Get();
    }
}