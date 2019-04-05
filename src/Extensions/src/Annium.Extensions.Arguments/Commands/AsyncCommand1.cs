using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Arguments
{
    public abstract class AsyncCommand<T> : CommandBase
    where T : new()
    {
        public abstract Task HandleAsync(T cfg, CancellationToken token);

        public override void Process(string command, string[] args, CancellationToken token)
        {
            if (Root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
            {
                Console.WriteLine(Root.HelpBuilder.BuildHelp(command, Description, typeof(T)));
                return;
            }

            var cfg = Root.ConfigurationBuilder.Build<T>(args);

            HandleAsync(cfg, token).GetAwaiter().GetResult();
        }
    }
}