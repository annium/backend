using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Extensions.Shell
{
    internal class ShellInstance : IShellInstance
    {
        private readonly string cmd;

        private readonly ILogger<Shell> logger;

        private ProcessStartInfo startInfo;

        private bool pipe;

        public ShellInstance(
            string cmd,
            ILogger<Shell> logger
        )
        {
            this.cmd = cmd;
            this.logger = logger;
        }

        public IShellInstance Configure(ProcessStartInfo startInfo)
        {
            this.startInfo = startInfo;

            return this;
        }

        public IShellInstance Pipe(bool pipe)
        {
            this.pipe = pipe;

            return this;
        }

        public Task<ShellResult> RunAsync(TimeSpan timeout)
        {
            var token = timeout == TimeSpan.Zero ?
                CancellationToken.None :
                new CancellationTokenSource(timeout).Token;

            return RunAsync(token);
        }

        public Task<ShellResult> RunAsync(CancellationToken token = default(CancellationToken))
        {
            var process = GetProcess();

            return StartProcess(process, token).Task;
        }

        public ShellAsyncResult Start(CancellationToken token = default(CancellationToken))
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
            var process = new Process();

            process.EnableRaisingEvents = true;

            if (startInfo != null)
                process.StartInfo = startInfo;

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            var args = cmd.Split(' ');
            process.StartInfo.FileName = args[0];
            process.StartInfo.Arguments = string.Join(" ", args.Skip(1));

            logger.Trace($"shell: {process.StartInfo.FileName} {process.StartInfo.Arguments}");

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
                logger.Trace($"Kill process {GetCommand(process)} due token cancellation");
                try
                {
                    process.Kill();
                }
                catch (Exception e)
                {
                    logger.Warn($"Process.Kill() failed: {e}");
                }
                handleExit();
            });

            process.Exited += (sender, e) =>
            {
                registration.Dispose();
                handleExit();
            };

            process.Start();

            if (pipe)
            {
                Task.Run(() => pipeOut(process.StandardOutput));
                Task.Run(() => pipeOut(process.StandardError));
            }

            return tcs;

            void handleExit()
            {
                if (exitHandled)
                    return;
                exitHandled = true;

                if (killed)
                    tcs.SetCanceled();
                else
                    tcs.SetResult(GetResult(process));
                try
                {
                    process.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn($"Process.Dispose() failed: {e}");
                }
            }

            void pipeOut(StreamReader src)
            {
                while (true)
                {
                    var line = src.ReadLine();
                    if (line == null)
                        return;

                    Console.WriteLine(line);
                }
            }
        }

        private ShellResult GetResult(Process process)
        {
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            return new ShellResult(process.ExitCode, output, error);
        }

        private string GetCommand(Process process) =>
            $"{process.StartInfo.FileName} {string.Join(' ',process.StartInfo.Arguments)}";
    }
}