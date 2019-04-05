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
            if (Root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
            {
                Console.WriteLine(Root.HelpBuilder.BuildHelp(command, Description));
                return;
            }

            HandleAsync(token).GetAwaiter().GetResult();
        }
    }
}