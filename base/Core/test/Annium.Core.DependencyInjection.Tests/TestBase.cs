using System;

namespace Annium.Core.DependencyInjection.Tests
{
    public class TestBase
    {
        protected readonly ServiceContainer _container = new();
        private IServiceProvider _provider = default!;

        protected void Build()
        {
            _provider = _container.BuildServiceProvider();
        }

        protected T Get<T>()
            where T : notnull
        {
            return _provider.Resolve<T>();
        }
    }
}