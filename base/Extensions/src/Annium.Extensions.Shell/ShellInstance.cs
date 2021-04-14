using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Extensions.Shell
{
    internal class ShellInstance : IShellInstance
    {
        private readonly object _consoleLock = new();
        private readonly string _cmd;
        private readonly ILogger<Shell> _logger;
        private ProcessStartInfo _startInfo;
        private bool _pipe;

        public ShellInstance(
            string cmd,
            ILogger<Shell> logger
        )
        {
            _cmd = cmd;
            _logger = logger;
            _startInfo = new ProcessStartInfo();
        }

        public IShellInstance Configure(ProcessStartInfo startInfo)
        {
            _startInfo = startInfo;

            return this;
        }

        public IShellInstance Pipe(bool pipe)
        {
            _pipe = pipe;

            return this;
        }

        public async Task<ShellResult> RunAsync(TimeSpan timeout)
        {
            if (timeout == TimeSpan.Zero)
                return await RunAsync(CancellationToken.None);

            using var cts = new CancellationTokenSource(timeout);

            return await RunAsync(cts.Token);
        }

        public async Task<ShellResult> RunAsync(CancellationToken token = default)
        {
            using var process = GetProcess();

            return await StartProcess(process, token).Task;
        }

        public ShellAsyncResult Start(CancellationToken token = default)
        {
            var process = GetProcess();

            var result = StartProcess(process, token).Task;

            return new ShellAsyncResult(
                process.StandardInput,
                process.StandardOutput,
                process.StandardError,
                result
            );
        }

        private Process GetProcess()
        {
            var process = new Process { EnableRaisingEvents = true };

            process.StartInfo = _startInfo;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            var args = _cmd.Split(' ');
            process.StartInfo.FileName = args[0];
            process.StartInfo.Arguments = string.Join(" ", args.Skip(1));

            _logger.Trace($"shell: {process.StartInfo.FileName} {process.StartInfo.Arguments}");

            return process;
        }

        private TaskCompletionSource<ShellResult> StartProcess(Process process, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<ShellResult>();

            // as far as there's no way to know if process was killed or finished on it's own - track it manually
            var killed = false;

            // this will be called when process finished on it's own, or is killed
            var exitHandled = false;

            // track token cancellation and kill process if requested
            var registration = token.Register(() =>
            {
                killed = true;
                _logger.Trace($"Kill process {GetCommand(process)} due token cancellation");
                try
                {
                    process.Kill();
                }
                catch (Exception e)
                {
                    _logger.Warn($"Kill process {GetCommand(process)} failed: {e}");
                }

                HandleExit();
            });

            process.Exited += (sender, e) =>
            {
                registration.Dispose();
                HandleExit();
            };

            process.Start();

            if (_pipe)
            {
                Task.Run(() =>
                {
                    lock (_consoleLock) PipeOut(process.StandardOutput);
                });
                Task.Run(() =>
                {
                    lock (_consoleLock) PipeOut(process.StandardError);
                });
                Task.Run(() => PipeOut(process.StandardError));
            }

            return tcs;

            void HandleExit()
            {
                if (exitHandled)
                    return;
                exitHandled = true;

                if (killed)
                    tcs!.SetCanceled();
                else
                    tcs!.SetResult(GetResult(process));
                try
                {
                    process.Dispose();
                }
                catch (Exception e)
                {
                    _logger.Warn($"Process.Dispose() failed: {e}");
                }
            }

            static void PipeOut(StreamReader src)
            {
                while (!src.EndOfStream)
                    Console.Write((char) src.Read());
            }
        }

        private ShellResult GetResult(Process process)
        {
            var output = Read(process.StandardOutput);
            var error = Read(process.StandardError);

            return new ShellResult(process.ExitCode, output, error);

            static string Read(StreamReader src)
            {
                var sb = new StringBuilder();
                string? line;
                while ((line = src.ReadLine()) != null)
                    sb.AppendLine(line);

                return sb.ToString();
            }
        }

        private string GetCommand(Process process) =>
            $"{process.StartInfo.FileName} {string.Join(' ', process.StartInfo.Arguments)}";
    }
}