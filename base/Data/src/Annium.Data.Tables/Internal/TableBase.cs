using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace Annium.Data.Tables.Internal
{
    internal abstract class TableBase<T> : IObservable<IChangeEvent<T>>
        where T : IEquatable<T>
    {
        protected readonly object DataLocker = new object();
        private readonly object _notificationLocker = new object();
        private readonly IObservable<IChangeEvent<T>> _observable;
        private readonly TablePermission _permissions;
        private readonly BlockingCollection<IChangeEvent<T>> _events;

        protected TableBase(TablePermission permissions)
        {
            _permissions = permissions;
            _events = new BlockingCollection<IChangeEvent<T>>(new ConcurrentQueue<IChangeEvent<T>>());
            _observable = CreateObservable();
        }

        public IDisposable Subscribe(IObserver<IChangeEvent<T>> observer) => _observable.Subscribe(observer);

        protected void AddEvents(IReadOnlyCollection<IChangeEvent<T>> events)
        {
            lock (_notificationLocker)
                foreach (var item in events)
                    _events.Add(item);
        }

        protected void AddEvent(IChangeEvent<T> @event)
        {
            lock (_notificationLocker)
                _events.Add(@event);
        }

        protected void EnsurePermission(TablePermission permission)
        {
            if (!_permissions.HasFlag(permission))
                throw new InvalidOperationException($"Table {GetType().FriendlyName()} has no {permission} permission.");
        }

        private IObservable<IChangeEvent<T>> CreateObservable() => Observable.Create(
            async (IObserver<IChangeEvent<T>> observer, CancellationToken token) =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var e = await Task.Run(() => _events.Take(), token);
                        observer.OnNext(e);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                }

                return Task.FromResult<Action>(() => { });
            }).Publish().RefCount();
    }
}
