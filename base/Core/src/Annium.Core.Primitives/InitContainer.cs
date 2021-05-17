using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Annium.Core.Primitives
{
    public class InitContainer<T>
    {
        public T Value
        {
            get
            {
                EnsureInitiated();
                return _get();
            }
        }

        public event Action<T> OnReady = delegate { };

        private AsyncLazy<T>? _initiator;

        // private
        private readonly Func<T> _get;
        private readonly Action<T> _set;

        public InitContainer()
        {
            var value = default(T)!;
            _get = () => value;
            _set = x => value = x;
        }

        public InitContainer(Func<T> get, Action<T> set)
        {
            _get = get;
            _set = set;
        }

        public void SetInit(Func<Task<T>> init)
        {
            if (_initiator is not null)
                throw new InvalidOperationException("Container is already initiated");

            _initiator = new AsyncLazy<T>(async () =>
            {
                var value = await init();
                _set(value);
                OnReady.Invoke(value);

                return value;
            });
        }

        public void Set(T value)
        {
            EnsureInitiated();

            _set(value);
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            EnsureInitiatorSet();

            return _initiator!.GetAwaiter();
        }

        private void EnsureInitiated()
        {
            EnsureInitiatorSet();

            if (!_initiator!.IsValueCreated)
                throw new InvalidOperationException("Container is not initiated");
        }

        private void EnsureInitiatorSet()
        {
            if (_initiator is null)
                throw new InvalidOperationException("Container initiator is not set");
        }

        public static implicit operator T(InitContainer<T> container) => container.Value;
    }
}