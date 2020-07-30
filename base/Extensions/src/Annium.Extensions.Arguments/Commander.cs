using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

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
            var group = _provider.GetRequiredService<TGroup>();
            group.SetRoot(_provider.GetRequiredService<Root>());
            group.Process(group.Id, args, token);
        }
    }
}