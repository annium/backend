using System.Threading;

namespace System
{
    public sealed record ObserverContext<T>
    {
        public Action<T> OnNext { get; }
        public Action<Exception> OnError { get; }
        public CancellationToken Token { get; init; }

        public ObserverContext(
            Action<T> onNext,
            Action<Exception> onError,
            CancellationToken token
        )
        {
            OnNext = onNext;
            OnError = onError;
            Token = token;
        }
    }
}