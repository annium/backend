using System;
using System.Runtime.Loader;
using System.Threading;

namespace Annium.Extensions.Entrypoint
{
    public partial class Entrypoint
    {
        private bool isAlreadyBuilt = false;

        private RunPack Build()
        {
            if (isAlreadyBuilt)
                throw new InvalidOperationException($"Entrypoint is already built");
            isAlreadyBuilt = true;

            var gate = new ManualResetEventSlim(false);

            return new RunPack(
                gate,
                GetCancellationToken(gate),
                serviceProviderBuilder.Build()
            );
        }

        private CancellationToken GetCancellationToken(ManualResetEventSlim gate)
        {
            var cts = new CancellationTokenSource();

            AssemblyLoadContext.Default.Unloading += (context) => HandleEnd();
            Console.CancelKeyPress += (sender, args) => HandleEnd();

            return cts.Token;

            void HandleEnd()
            {
                cts.Cancel();
                gate.Wait();
            }
        }
    }
}