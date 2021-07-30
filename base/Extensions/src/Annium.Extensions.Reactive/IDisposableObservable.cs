namespace System
{
    public interface IDisposableObservable<out T> : IObservable<T>, IDisposable
    {
    }
}