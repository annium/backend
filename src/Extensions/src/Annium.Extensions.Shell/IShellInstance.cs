using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Shell
{
    public interface IShellInstance
    {
        IShellInstance Configure(ProcessStartInfo startInfo);

        Task<ShellResult> RunAsync(TimeSpan timeout);

        Task<ShellResult> RunAsync(CancellationToken token = default(CancellationToken));

        ShellAsyncResult Start(CancellationToken token = default(CancellationToken));
    }
}