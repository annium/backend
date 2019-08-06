using System;
using System.Threading;

namespace Annium.Extensions.Arguments
{
    public abstract class Command : CommandBase
    {
        public abstract void Handle(CancellationToken token);

        public override void Process(string command, string[] args, CancellationToken token)
        {
            if (Root.ConfigurationBuilder.Build<HelpConfiguration>(args).Help)
            {
                Console.WriteLine(Root.HelpBuilder.BuildHelp(command, Description));
                return;
            }

            Handle(token);
        }
    }
}