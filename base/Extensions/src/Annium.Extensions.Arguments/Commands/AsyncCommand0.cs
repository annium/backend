using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Arguments
{
    public abstract class AsyncCommand : CommandBase
    {
        public abstract Task HandleAsync(CancellationToken token);

        public override void Process(string command, string[] args, CancellationToken token)
        {
            var root = Root!;
            if (root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
            {
                Console.WriteLine(root.HelpBuilder.BuildHelp(command, Description));
                return;
            }

            HandleAsync(token).GetAwaiter().GetResult();
        }
    }
}