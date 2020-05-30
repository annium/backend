using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Arguments
{
    public abstract class AsyncCommand<T1, T2> : CommandBase
        where T1 : new()
        where T2 : new()
    {
        public abstract Task HandleAsync(T1 cfg1, T2 cfg2, CancellationToken token);

        public override void Process(string command, string[] args, CancellationToken token)
        {
            var root = Root!;
            if (root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
            {
                Console.WriteLine(root.HelpBuilder.BuildHelp(command, Description, typeof(T1), typeof(T2)));
                return;
            }

            var cfg1 = root.ConfigurationBuilder.Build<T1>(args);
            var cfg2 = root.ConfigurationBuilder.Build<T2>(args);

            HandleAsync(cfg1, cfg2, token).GetAwaiter().GetResult();
        }
    }
}