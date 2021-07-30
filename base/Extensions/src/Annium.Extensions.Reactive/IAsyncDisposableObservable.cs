namespace System
{
    public interface IAsyncDisposableObservable<out T> : IObservable<T>, IAsyncDisposable
    {
    }
}