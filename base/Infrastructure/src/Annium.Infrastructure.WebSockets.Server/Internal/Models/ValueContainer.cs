using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Models
{
    internal class ValueContainer<TValueLoader, TConfig, TValue> : ValueContainerBase<TValue>, IValueContainer<TConfig, TValue>
        where TValueLoader : IValueLoader<TConfig, TValue>
    {
        private bool _isConfigured;
        private TConfig? _config;

        public ValueContainer(
            IServiceProvider sp
        ) : base(sp)
        {
        }

        public void Configure(TConfig config)
        {
            if (_isConfigured)
                throw new InvalidOperationException("Container already configured");
            _isConfigured = true;

            _config = config;
        }

        protected override async Task<TValue> LoadValueAsync(IAsyncServiceScope scope)
        {
            if (!_isConfigured)
                throw new InvalidOperationException("Container is not configured");
            var config = _config!;

            var loader = scope.ServiceProvider.Resolve<TValueLoader>();
            var value = await loader.LoadAsync(config);

            return value;
        }
    }

    internal class ValueContainer<TValueLoader, TValue> : ValueContainerBase<TValue>, IValueContainer<TValue>
        where TValueLoader : IValueLoader<TValue>
    {
        public ValueContainer(
            IServiceProvider sp
        ) : base(sp)
        {
        }

        protected override async Task<TValue> LoadValueAsync(IAsyncServiceScope scope)
        {
            var loader = scope.ServiceProvider.Resolve<TValueLoader>();
            var value = await loader.LoadAsync();

            return value;
        }
    }

    internal abstract class ValueContainerBase<TValue>
    {
        public TValue Value
        {
            get
            {
                EnsureInitiated();
                return _value;
            }
        }

        public event Action<TValue> OnChange = delegate { };

        private readonly IServiceProvider _sp;
        private readonly AsyncLazy<TValue> _initiator;
        private TValue _value;

        protected ValueContainerBase(
            IServiceProvider sp
        )
        {
            _sp = sp;
            _initiator = new AsyncLazy<TValue>((Func<Task<TValue>>) LoadValueAsync);
            _value = default!;
        }

        public void Set(TValue value)
        {
            EnsureInitiated();

            _value = value;
            OnChange.Invoke(value);
        }


        public TaskAwaiter<TValue> GetAwaiter()
        {
            return _initiator.IsValueCreated
                ? Task.FromResult(_value).GetAwaiter()
                : _initiator.GetAwaiter();
        }

        protected abstract Task<TValue> LoadValueAsync(IAsyncServiceScope scope);

        private async Task<TValue> LoadValueAsync()
        {
            await using var scope = _sp.CreateAsyncScope();

            var value = await LoadValueAsync(scope);

            _value = value;
            OnChange.Invoke(value);

            return value;
        }

        private void EnsureInitiated()
        {
            if (!_initiator.IsValueCreated)
                throw new InvalidOperationException("Container is not initiated");
        }
    }
}