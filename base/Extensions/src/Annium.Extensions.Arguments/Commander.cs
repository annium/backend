using System;
using System.Threading;
using Annium.Core.DependencyInjection;

namespace Annium.Extensions.Arguments
{
    public class Commander
    {
        private readonly IServiceProvider _provider;

        public Commander(IServiceProvider provider)
        {
            _provider = provider;
        }

        public void Run<TGroup>(string[] args, CancellationToken token = default)
            where TGroup : Group
        {
            var group = _provider.Resolve<TGroup>();
            group.SetRoot(_provider.Resolve<Root>());
            group.Process(group.Id, args, token);
        }
    }
}