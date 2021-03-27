using System.Runtime.CompilerServices;

namespace Annium.Core.Primitives
{
    public interface IAwaiter<T> : ICriticalNotifyCompletion
    {
        bool IsCompleted { get; }
        T GetResult();
    }

    public interface IAwaiter : ICriticalNotifyCompletion
    {
        bool IsCompleted { get; }
        void GetResult();
    }
}