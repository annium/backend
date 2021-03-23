using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class WorkScheduler : IAsyncDisposable
    {
        private readonly IAsyncDisposableBox _disposable = Disposable.AsyncBox();
        private readonly IList<Func<Task>> _backlog = new List<Func<Task>>();
        private int _isStarted;

        public void Add(Func<Task> work)
        {
            if (_isStarted == 1)
                Schedule(work);
            else
                _backlog.Add(work);
        }

        public void Start()
        {
            if (Interlocked.CompareExchange(ref _isStarted, 1, 0) == 1)
                throw new InvalidOperationException("Can't start WorkScheduler twice");
            foreach (var work in _backlog)
                Schedule(work);
        }

        public async ValueTask DisposeAsync()
        {
            _backlog.Clear();
            await _disposable.DisposeAsync();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Schedule(Func<Task> work)
        {
            var task = Task.Run(work);
            _disposable.Add(() => task);
        }
    }
}