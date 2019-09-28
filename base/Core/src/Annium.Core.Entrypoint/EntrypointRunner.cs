using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Core.Entrypoint
{
    public partial class Entrypoint
    {
        public int Run(Action<IServiceProvider, string[], CancellationToken> main, string[] args) =>
            InternalRun(main, args);

        public int Run(Action<IServiceProvider, string[]> main, string[] args) =>
            InternalRun((provider, arguments, token) => main(provider, arguments), args);

        public int Run(Action<IServiceProvider, CancellationToken> main) =>
            InternalRun((provider, arguments, token) => main(provider, token), Array.Empty<string>());

        public int Run(Action<IServiceProvider> main) =>
            InternalRun((provider, arguments, token) => main(provider), Array.Empty<string>());

        public int RunAsync(Func<IServiceProvider, string[], CancellationToken, Task> main, string[] args) =>
            InternalRunAsync(main, args).GetAwaiter().GetResult();

        public int RunAsync(Func<IServiceProvider, string[], Task> main, string[] args) =>
            InternalRunAsync((provider, arguments, token) => main(provider, arguments), args).GetAwaiter().GetResult();

        public int RunAsync(Func<IServiceProvider, CancellationToken, Task> main) =>
            InternalRunAsync((provider, arguments, token) => main(provider, token), Array.Empty<string>()).GetAwaiter().GetResult();

        public int RunAsync(Func<IServiceProvider, Task> main) =>
            InternalRunAsync((provider, arguments, token) => main(provider), Array.Empty<string>()).GetAwaiter().GetResult();

        private int InternalRun(
            Action<IServiceProvider, string[], CancellationToken> main,
            string[] args
        )
        {
            var (gate, token, provider) = Build();

            try
            {
                main(provider, args, token);

                return 0;
            }
            catch (AggregateException exception)
            {
                LogAggregateException(exception);
                return 1;
            }
            catch (Exception exception)
            {
                LogException(exception);
                return 1;
            }
            finally
            {
                if (provider is IDisposable disposableProvider)
                    disposableProvider.Dispose();
                gate.Set();
            }
        }

        private async Task<int> InternalRunAsync(
            Func<IServiceProvider, string[], CancellationToken, Task> main,
            string[] args
        )
        {
            var (gate, token, provider) = Build();

            try
            {
                await main(provider, args, token);

                return 0;
            }
            catch (AggregateException exception)
            {
                LogAggregateException(exception);
                return 1;
            }
            catch (Exception exception)
            {
                LogException(exception);
                return 1;
            }
            finally
            {
                if (provider is IDisposable disposableProvider)
                    disposableProvider.Dispose();
                gate.Set();
            }
        }

        private void LogAggregateException(AggregateException aggregateException)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            var exceptions = aggregateException.Flatten().InnerExceptions;
            Console.WriteLine($"Errors ({exceptions.Count}):");
            foreach (var exception in exceptions)
                Console.WriteLine(exception);

            Console.ForegroundColor = color;
        }

        private void LogException(Exception exception)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(exception);

            Console.ForegroundColor = color;
        }
    }
}