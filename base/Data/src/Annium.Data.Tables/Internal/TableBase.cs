using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace Annium.Data.Tables.Internal
{
    internal abstract class TableBase<T> : ITableView<T>
        where T : IEquatable<T>, ICopyable<T>
    {
        public abstract int Count { get; }
        protected readonly object DataLocker = new();
        private readonly IObservable<IChangeEvent<T>> _observable;
        private readonly TablePermission _permissions;
        private readonly ChannelWriter<IChangeEvent<T>> _eventWriter;
        private readonly ChannelReader<IChangeEvent<T>> _eventReader;
        private readonly AsyncDisposableBox _disposable = Disposable.AsyncBox();

        protected TableBase(TablePermission permissions)
        {
            _permissions = permissions;
            var taskChannel = Channel.CreateUnbounded<IChangeEvent<T>>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleReader = true,
                SingleWriter = true
            });
            _eventWriter = taskChannel.Writer;
            _eventReader = taskChannel.Reader;

            var observable = CreateObservable();
            _disposable += observable;
            _observable = observable.ObserveOn(TaskPoolScheduler.Default);
        }

        public IDisposable Subscribe(IObserver<IChangeEvent<T>> observer)
        {
            var init = ChangeEvent.Init(Get());
            observer.OnNext(init);

            return _observable.Subscribe(observer);
        }

        protected void AddEvents(IReadOnlyCollection<IChangeEvent<T>> events)
        {
            foreach (var @event in events)
                if (!_eventWriter.TryWrite(@event))
                    throw new InvalidOperationException("Event must have been sent.");
        }

        protected void AddEvent(IChangeEvent<T> @event)
        {
            if (!_eventWriter.TryWrite(@event))
                throw new InvalidOperationException("Event must have been sent.");
        }

        protected void EnsurePermission(TablePermission permission)
        {
            if (!_permissions.HasFlag(permission))
                throw new InvalidOperationException($"Table {GetType().FriendlyName()} has no {permission} permission.");
        }

        protected abstract IReadOnlyCollection<T> Get();

        private IAsyncDisposableObservable<IChangeEvent<T>> CreateObservable() => ObservableExt.StaticSyncInstance<IChangeEvent<T>>(async ctx =>
        {
            try
            {
                while (!ctx.Ct.IsCancellationRequested)
                {
                    var message = await _eventReader.ReadAsync(ctx.Ct);

                    ctx.OnNext(message);
                }
            }
            // token was canceled
            catch (OperationCanceledException)
            {
                ctx.OnCompleted();
            }
            catch (ChannelClosedException)
            {
                ctx.OnCompleted();
            }
            catch (Exception e)
            {
                ctx.OnError(e);
            }

            return () => Task.CompletedTask;
        });

        public IEnumerator<T> GetEnumerator() => Get().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Get().GetEnumerator();

        public virtual ValueTask DisposeAsync()
        {
            _eventWriter.Complete();
            return _disposable.DisposeAsync();
        }
    }
}