using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Shell
{
    public interface IShellInstance
    {
        IShellInstance Configure(ProcessStartInfo startInfo);

        IShellInstance Pipe(bool pipe);

        Task<ShellResult> RunAsync(TimeSpan timeout);

        Task<ShellResult> RunAsync(CancellationToken token = default);

        ShellAsyncResult Start(CancellationToken token = default);
    }
}