using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.Entrypoint
{
    internal class RunPack
    {
        public ManualResetEventSlim Gate { get; }

        public CancellationToken Token { get; }

        public ServiceProvider Provider { get; }

        public RunPack(
            ManualResetEventSlim gate,
            CancellationToken token,
            ServiceProvider provider)
        {
            Gate = gate;
            Token = token;
            Provider = provider;
        }

        public void Deconstruct(
            out ManualResetEventSlim gate,
            out CancellationToken token,
            out ServiceProvider provider
        )
        {
            gate = Gate;
            token = Token;
            provider = Provider;
        }
    }
}