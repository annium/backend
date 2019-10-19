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

        public int Run(Action<string[], CancellationToken> main, string[] args) =>
            InternalRun((provider, arguments, token) => main(arguments, token), args);

        public int Run(Action<string[]> main, string[] args) =>
            InternalRun((provider, arguments, token) => main(arguments), args);

        public int Run(Action<CancellationToken> main) =>
            InternalRun((provider, arguments, token) => main(token), Array.Empty<string>());

        public int Run(Action main) =>
            InternalRun((provider, arguments, token) => main(), Array.Empty<string>());

        public Task<int> Run(Func<IServiceProvider, string[], CancellationToken, Task> main, string[] args) =>
            InternalRun(main, args);

        public Task<int> Run(Func<IServiceProvider, string[], Task> main, string[] args) =>
            InternalRun((provider, arguments, token) => main(provider, arguments), args);

        public Task<int> Run(Func<IServiceProvider, CancellationToken, Task> main) =>
            InternalRun((provider, arguments, token) => main(provider, token), Array.Empty<string>());

        public Task<int> Run(Func<IServiceProvider, Task> main) =>
            InternalRun((provider, arguments, token) => main(provider), Array.Empty<string>());

        public Task<int> Run(Func<string[], CancellationToken, Task> main, string[] args) =>
            InternalRun((provider, arguments, token) => main(arguments, token), args);

        public Task<int> Run(Func<string[], Task> main, string[] args) =>
            InternalRun((provider, arguments, token) => main(arguments), args);

        public Task<int> Run(Func<CancellationToken, Task> main) =>
            InternalRun((provider, arguments, token) => main(token), Array.Empty<string>());

        public Task<int> Run(Func<Task> main) =>
            InternalRun((provider, arguments, token) => main(), Array.Empty<string>());

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

        private async Task<int> InternalRun(
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