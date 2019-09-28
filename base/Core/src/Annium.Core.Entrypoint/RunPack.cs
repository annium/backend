using System;
using System.Threading;

namespace Annium.Core.Entrypoint
{
    internal class RunPack
    {
        public ManualResetEventSlim Gate { get; }

        public CancellationToken Token { get; }

        public IServiceProvider Provider { get; }

        public RunPack(
            ManualResetEventSlim gate,
            CancellationToken token,
            IServiceProvider provider
        )
        {
            Gate = gate;
            Token = token;
            Provider = provider;
        }

        public void Deconstruct(
            out ManualResetEventSlim gate,
            out CancellationToken token,
            out IServiceProvider provider
        )
        {
            gate = Gate;
            token = Token;
            provider = Provider;
        }
    }
}