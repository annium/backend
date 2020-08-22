using System;
using System.Runtime.CompilerServices;

namespace Annium.Core
{
    public sealed class TrackingWeakReference<T>
        where T : class?
    {
        public static readonly ConditionalWeakTable<T, TrackingWeakReference<T>> Registry = new ConditionalWeakTable<T, TrackingWeakReference<T>>();

        private readonly Action _handle;
        private readonly WeakReference<T> _ref;

        public bool IsAlive => TryGetTarget(out _);

        public TrackingWeakReference(T target, Action handle) : this(target, false, handle)
        {
        }

        public TrackingWeakReference(T target, bool trackResurrection, Action handle)
        {
            _handle = handle;
            _ref = new WeakReference<T>(target, trackResurrection);
            Registry.AddOrUpdate(target, this);
        }

        public bool TryGetTarget(out T target) => _ref.TryGetTarget(out target);

        ~TrackingWeakReference()
        {
            _handle();
        }
    }
}