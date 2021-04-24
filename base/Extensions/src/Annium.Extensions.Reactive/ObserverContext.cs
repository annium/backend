using System.Threading;

namespace System
{
    public sealed record ObserverContext<T>
    {
        public Action<T> OnNext { get; }
        public Action<Exception> OnError { get; }
        public Action OnCompleted { get; }
        public CancellationToken Token { get; init; }

        public ObserverContext(
            Action<T> onNext,
            Action<Exception> onError,
            Action onCompleted,
            CancellationToken token
        )
        {
            OnNext = onNext;
            OnError = onError;
            OnCompleted = onCompleted;
            Token = token;
        }
    }
}