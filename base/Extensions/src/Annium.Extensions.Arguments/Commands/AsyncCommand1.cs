using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Extensions.Arguments.Internal;

namespace Annium.Extensions.Arguments
{
    public abstract class AsyncCommand<T> : CommandBase
        where T : new()
    {
        public abstract Task HandleAsync(T cfg, CancellationToken token);

        public override void Process(string command, string[] args, CancellationToken token)
        {
            var root = Root!;
            if (root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
            {
                Console.WriteLine(root.HelpBuilder.BuildHelp(command, Description, typeof(T)));
                return;
            }

            var cfg = root.ConfigurationBuilder.Build<T>(args);

            HandleAsync(cfg, token).Await();
        }
    }
}