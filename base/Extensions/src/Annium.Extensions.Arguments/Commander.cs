﻿using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.Arguments
{
    public class Commander
    {
        private readonly IServiceProvider provider;

        public Commander(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public void Run<TGroup>(string[] args, CancellationToken token = default(CancellationToken))
        where TGroup : Group
        {
            var group = provider.GetRequiredService<TGroup>();
            group.Root = provider.GetRequiredService<Root>();
            group.Process(group.Id, args, token);
        }
    }
}