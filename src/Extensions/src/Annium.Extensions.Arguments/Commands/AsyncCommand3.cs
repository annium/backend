using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Arguments
{
    public abstract class AsyncCommand<T1, T2, T3> : CommandBase
    where T1 : new()
    where T2 : new()
    where T3 : new()
    {
        public abstract Task HandleAsync(T1 cfg1, T2 cfg2, T3 cfg3, CancellationToken token);

        public override void Process(string command, string[] args, CancellationToken token)
        {
            if (Root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
            {
                Console.WriteLine(Root.HelpBuilder.BuildHelp(command, Description, typeof(T1), typeof(T2), typeof(T3)));
                return;
            }

            var cfg1 = Root.ConfigurationBuilder.Build<T1>(args);
            var cfg2 = Root.ConfigurationBuilder.Build<T2>(args);
            var cfg3 = Root.ConfigurationBuilder.Build<T3>(args);

            HandleAsync(cfg1, cfg2, cfg3, token).GetAwaiter().GetResult();
        }
    }
}